using Domain.Common;
using Domain.Errors;
using Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class Book : BaseEntity<long>, IAuditable
    {
        private readonly List<BookAuthor> _bookAuthors = new List<BookAuthor>();
        private readonly List<BookGenre> _bookGenres = new List<BookGenre>();

        public Isbn Isbn { get; set; }
        public string Title { get; set; }
        public int PublisherId { get; set; }
        public short PublicationYear { get; set; }
        public bool IsActive { get; private set; }
        public IReadOnlyCollection<BookAuthor> BookAuthors => _bookAuthors.AsReadOnly();
        public IReadOnlyCollection<BookGenre> BookGenres => _bookGenres.AsReadOnly();

        public Guid? CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; }
        public Guid? ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }

        public static Result<Book> Create(Isbn isbn, string title, int publisherId, short publicationYear)
        {
            if (isbn is null) return Result<Book>.Failure(BookErrors.IsbnNotProvided);
            if (string.IsNullOrWhiteSpace(title)) return Result<Book>.Failure(BookErrors.TitleNotProvided);
            if (publisherId <= 0) return Result<Book>.Failure(BookErrors.PublisherNotProvided);
            if (publicationYear <= 0 || publicationYear > DateTime.UtcNow.Year) return Result<Book>.Failure(BookErrors.InvalidPublicationYear);

            Book book = new Book()
            {
                Isbn = isbn,
                Title = title,
                PublisherId = publisherId,
                PublicationYear = publicationYear,
                IsActive = true
            };

            return Result<Book>.Success(book);
        }

        public Result AddAuthor(int authorId)
        {
            if (_bookAuthors.Any(a => a.AuthorId == authorId)) return Result.Failure(BookErrors.AuthorAlreadyAdded);

            _bookAuthors.Add(BookAuthor.Create(authorId));

            return Result.Success();
        }

        public Result RemoveAuthor(int authorId)
        {
            BookAuthor bookAuthor = _bookAuthors.FirstOrDefault(a => a.AuthorId == authorId);
            if (bookAuthor is null) return Result.Failure(BookErrors.AuthorNotFound);

            _bookAuthors.Remove(bookAuthor);

            return Result.Success();
        }

        public Result AddGenre(int genreId)
        {
            if (_bookGenres.Any(g => g.GenreId == genreId)) return Result.Failure(BookErrors.GenreAlreadyAdded);

            _bookGenres.Add(BookGenre.Create(genreId));

            return Result.Success();
        }

        public Result RemoveGenre(int genreId)
        {
            BookGenre bookGenre = _bookGenres.FirstOrDefault(g => g.GenreId == genreId);
            if (bookGenre is null) return Result.Failure(BookErrors.GenreNotFound);

            _bookGenres.Remove(bookGenre);

            return Result.Success();
        }

        public void SetActive(bool active) => IsActive = active;
    }
}
