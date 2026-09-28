using Application.Common;
using Application.Errors;
using Application.Persistance;
using AutoMapper;
using Domain.Common;
using Domain.Entities;
using Domain.Enums;
using Domain.Errors;
using Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.Books
{
    public class BookService
    {
        private const string _includes = $"{nameof(Book.BookAuthors)},{nameof(Book.BookGenres)}";

        private readonly IBookRepository _repository;
        private readonly IBookCopyRepository _bookCopyRepository;
        private readonly IPublisherRepository _publisherRepository;
        private readonly IAuthorRepository _authorRepository;
        private readonly IGenreRepository _genreRepository;
        private readonly IMapper _mapper;
        private readonly BookValidator _validator;
        private readonly SearchBooksValidator _searchValidator;
        public BookService(IBookRepository repository, IBookCopyRepository bookCopyRepository, IPublisherRepository publisherRepository,
            IAuthorRepository authorRepository, IGenreRepository genreRepository, IMapper mapper, BookValidator validator, SearchBooksValidator searchValidator)
        {
            _searchValidator = searchValidator;
            _repository = repository;
            _bookCopyRepository = bookCopyRepository;
            _publisherRepository = publisherRepository;
            _authorRepository = authorRepository;
            _genreRepository = genreRepository;
            _mapper = mapper;
            _validator = validator;
        }

        public async Task<Result<BookResponse>> GetBook(long id, CancellationToken cancellationToken = default)
        {
            Book book = await _repository.GetAsync(b => b.Id == id, _includes, cancellationToken: cancellationToken);
            if (book is null) return Result<BookResponse>.Failure(RepositoryErrors.EntityNotFound);

            List<BookResponse> responses = await ToResponses(new List<Book> { book }, cancellationToken);

            return Result<BookResponse>.Success(responses[0]);
        }

        public async Task<Result<PagedResponse<BookResponse>>> SearchBooks(SearchBooksRequest request, CancellationToken cancellationToken = default)
        {
            var validateResult = await _searchValidator.ValidateAsync(request, cancellationToken);
            if (!validateResult.IsValid) return ValidationResult<PagedResponse<BookResponse>>.From(validateResult.Errors);

            // Search text that is a valid ISBN matches that book exactly, anything else matches on title.
            string search = string.IsNullOrWhiteSpace(request.Search) ? null : request.Search.Trim();
            Result<Isbn> isbnResult = Isbn.Create(search);
            Isbn isbn = isbnResult.IsSuccess ? isbnResult.Value : null;
            bool includeInactive = request.IncludeInactive;
            int? publisherId = request.PublisherId;
            int? authorId = request.AuthorId;
            int? genreId = request.GenreId;

            Expression<Func<Book, bool>> filter = b =>
                (includeInactive || b.IsActive) &&
                (isbn != null ? b.Isbn == isbn : search == null || b.Title.Contains(search)) &&
                // Get all if request param is null (not provided)
                (publisherId == null || b.PublisherId == publisherId) &&
                (authorId == null || b.BookAuthors.Any(a => a.AuthorId == authorId)) &&
                (genreId == null || b.BookGenres.Any(g => g.GenreId == genreId));

            int totalCount = await _repository.CountAsync(filter, cancellationToken);
            IEnumerable<Book> books = await _repository.GetAllAsync(filter, _includes, request.PageSize, request.PageNo,
                orderBy: q => q.OrderBy(b => b.Title).ThenBy(b => b.Id), tracked: false, cancellationToken: cancellationToken);

            return Result<PagedResponse<BookResponse>>.Success(new PagedResponse<BookResponse>
            {
                Items = await ToResponses(books.ToList(), cancellationToken),
                PageNo = request.PageNo,
                PageSize = request.PageSize,
                TotalCount = totalCount
            });
        }

        public async Task<Result<BookResponse>> AddBook(SaveBookRequest request, CancellationToken cancellationToken = default)
        {
            var validateResult = await _validator.ValidateAsync(request, cancellationToken);
            if (!validateResult.IsValid) return ValidationResult<BookResponse>.From(validateResult.Errors);

            Result<Isbn> isbnResult = Isbn.Create(request.Isbn);
            if (isbnResult.IsFailure) return Result<BookResponse>.Failure(isbnResult.Error);

            Result referenceResult = await CheckReferences(request, isbnResult.Value, 0, cancellationToken);
            if (referenceResult.IsFailure) return Result<BookResponse>.Failure(referenceResult.Error);

            Result<Book> createResult = Book.Create(isbnResult.Value, request.Title, request.PublisherId, request.PublicationYear);
            if (createResult.IsFailure) return Result<BookResponse>.Failure(createResult.Error);

            Book book = createResult.Value;

            Result authorsResult = SyncAuthors(book, request.AuthorIds);
            if (authorsResult.IsFailure) return Result<BookResponse>.Failure(authorsResult.Error);

            Result genresResult = SyncGenres(book, request.GenreIds);
            if (genresResult.IsFailure) return Result<BookResponse>.Failure(genresResult.Error);

            await _repository.AddAsync(book, cancellationToken);
            await _repository.CommitAsync(cancellationToken);

            return Result<BookResponse>.Success(_mapper.Map<BookResponse>(book));
        }

        public async Task<Result<BookResponse>> UpdateBook(long id, SaveBookRequest request, CancellationToken cancellationToken = default)
        {
            var validateResult = await _validator.ValidateAsync(request, cancellationToken);
            if (!validateResult.IsValid) return ValidationResult<BookResponse>.From(validateResult.Errors);

            Result<Isbn> isbnResult = Isbn.Create(request.Isbn);
            if (isbnResult.IsFailure) return Result<BookResponse>.Failure(isbnResult.Error);

            Book book = await _repository.GetAsync(b => b.Id == id, _includes, tracked: true, cancellationToken: cancellationToken);
            if (book is null) return Result<BookResponse>.Failure(RepositoryErrors.EntityNotFound);

            Result referenceResult = await CheckReferences(request, isbnResult.Value, book.Id, cancellationToken);
            if (referenceResult.IsFailure) return Result<BookResponse>.Failure(referenceResult.Error);

            book.Isbn = isbnResult.Value;
            book.Title = request.Title;
            book.PublisherId = request.PublisherId;
            book.PublicationYear = request.PublicationYear;

            Result authorsResult = SyncAuthors(book, request.AuthorIds);
            if (authorsResult.IsFailure) return Result<BookResponse>.Failure(authorsResult.Error);

            Result genresResult = SyncGenres(book, request.GenreIds);
            if (genresResult.IsFailure) return Result<BookResponse>.Failure(genresResult.Error);

            _repository.Update(book);
            await _repository.CommitAsync(cancellationToken);

            return Result<BookResponse>.Success(_mapper.Map<BookResponse>(book));
        }

        public async Task<Result> DeleteBook(long id, CancellationToken cancellationToken = default)
        {
            Book book = await _repository.GetAsync(b => b.Id == id, tracked: true, cancellationToken: cancellationToken);
            if (book is null) return Result.Failure(RepositoryErrors.EntityNotFound);

            List<BookCopy> copies = (await _bookCopyRepository.GetAllAsync(c => c.BookId == id, cancellationToken: cancellationToken)).ToList();
            if (copies.Any(c => c.IsOnLoan)) return Result.Failure(BookErrors.HasBorrowedCopies);

            book.SetActive(false);
            foreach (BookCopy copy in copies)
            {
                copy.Withdraw();
            }

            _repository.Update(book);
            _bookCopyRepository.UpdateRange(copies);
            await _repository.CommitAsync(cancellationToken);

            return Result.Success();
        }

        private async Task<List<BookResponse>> ToResponses(List<Book> books, CancellationToken cancellationToken)
        {
            List<BookResponse> responses = _mapper.Map<List<BookResponse>>(books);
            List<long> bookIds = books.Select(b => b.Id).ToList();
            Dictionary<long, int> availableCounts = await _bookCopyRepository.CountAvailableByBook(bookIds, cancellationToken);

            foreach (BookResponse response in responses)
            {
                response.AvailableCopyCount = availableCounts.GetValueOrDefault(response.Id);
            }

            return responses;
        }

        private async Task<Result> CheckReferences(SaveBookRequest request, Isbn isbn, long bookId, CancellationToken cancellationToken)
        {
            if (await _repository.IsbnExists(isbn, bookId, cancellationToken)) return Result.Failure(BookServiceErrors.IsbnAlreadyExists);

            if (!await _publisherRepository.Exists(request.PublisherId, cancellationToken)) return Result.Failure(BookServiceErrors.PublisherNotFound);

            if (!await _authorRepository.AllExist(request.AuthorIds.Distinct().ToList(), cancellationToken)) return Result.Failure(BookServiceErrors.AuthorNotFound);

            if (!await _genreRepository.AllExist(request.GenreIds.Distinct().ToList(), cancellationToken)) return Result.Failure(BookServiceErrors.GenreNotFound);

            return Result.Success();
        }

        private static Result SyncAuthors(Book book, IEnumerable<int> requestedAuthorIds)
        {
            HashSet<int> authorIds = requestedAuthorIds.ToHashSet();
            foreach (int authorId in book.BookAuthors.Select(a => a.AuthorId).Where(id => !authorIds.Contains(id)).ToList())
            {
                Result result = book.RemoveAuthor(authorId);
                if (result.IsFailure) return result;
            }
            foreach (int authorId in authorIds.Where(id => book.BookAuthors.All(a => a.AuthorId != id)).ToList())
            {
                Result result = book.AddAuthor(authorId);
                if (result.IsFailure) return result;
            }

            return Result.Success();
        }

        private static Result SyncGenres(Book book, IEnumerable<int> requestedGenreIds)
        {
            HashSet<int> genreIds = requestedGenreIds.ToHashSet();
            foreach (int genreId in book.BookGenres.Select(g => g.GenreId).Where(id => !genreIds.Contains(id)).ToList())
            {
                Result result = book.RemoveGenre(genreId);
                if (result.IsFailure) return result;
            }
            foreach (int genreId in genreIds.Where(id => book.BookGenres.All(g => g.GenreId != id)).ToList())
            {
                Result result = book.AddGenre(genreId);
                if (result.IsFailure) return result;
            }

            return Result.Success();
        }
    }
}
