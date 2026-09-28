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

namespace Application.Features.Borrowings
{
    public class BorrowingService
    {
        private static readonly TimeSpan _loanPeriod = TimeSpan.FromDays(14);
        private const string _includes = $"{nameof(Borrowing.User)},{nameof(Borrowing.BookCopy)}.{nameof(BookCopy.Book)}";

        private readonly IBorrowingRepository _repository;
        private readonly IBookCopyRepository _bookCopyRepository;
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;
        private readonly BorrowValidator _borrowValidator;
        private readonly AssignBorrowValidator _assignValidator;
        private readonly SearchBorrowingsValidator _searchValidator;
        private readonly TimeProvider _timeProvider;
        private DateTime Now => _timeProvider.GetUtcNow().UtcDateTime;

        public BorrowingService(IBorrowingRepository repository,
            IBookCopyRepository bookCopyRepository,
            IUserRepository userRepository,
            IMapper mapper,
            BorrowValidator borrowValidator,
            AssignBorrowValidator assignValidator,
            SearchBorrowingsValidator searchValidator,
            TimeProvider timeProvider)
        {
            _repository = repository;
            _bookCopyRepository = bookCopyRepository;
            _userRepository = userRepository;
            _mapper = mapper;
            _borrowValidator = borrowValidator;
            _assignValidator = assignValidator;
            _searchValidator = searchValidator;
            _timeProvider = timeProvider;
        }

        // When ownerId is set, the borrowing must belong to that user.
        public async Task<Result<BorrowingResponse>> GetBorrowing(Guid id, Guid? ownerId = null, CancellationToken cancellationToken = default)
        {
            Borrowing borrowing = await _repository.GetAsync(b => b.Id == id && (ownerId == null || b.UserId == ownerId), _includes, cancellationToken: cancellationToken);
            if (borrowing is null) return Result<BorrowingResponse>.Failure(RepositoryErrors.EntityNotFound);

            return Result<BorrowingResponse>.Success(ToResponse(borrowing, borrowing.BookCopy?.AvailableStatus, Now));
        }

        // Full history for librarians, or personal history when UserId is the current user.
        public async Task<Result<PagedResponse<BorrowingResponse>>> SearchBorrowings(SearchBorrowingsRequest request, CancellationToken cancellationToken = default)
        {
            var validateResult = await _searchValidator.ValidateAsync(request, cancellationToken);
            if (!validateResult.IsValid) return ValidationResult<PagedResponse<BorrowingResponse>>.From(validateResult.Errors);

            // Search text that is a valid ISBN matches that book exactly, anything else matches on title or borrower name.
            string search = string.IsNullOrWhiteSpace(request.Search) ? null : request.Search.Trim();
            Result<Isbn> isbnResult = Isbn.Create(search);
            Isbn isbn = isbnResult.IsSuccess ? isbnResult.Value : null;
            Guid? userId = request.UserId;
            BorrowingStatus? status = request.Status;
            DateTime now = Now;

            // Status rules must match GetStatus.
            Expression<Func<Borrowing, bool>> filter = b =>
                (userId == null || b.UserId == userId) &&
                (status == null ||
                    (status == BorrowingStatus.Returned && b.ReturnDate != null) ||
                    (status == BorrowingStatus.ReturnRequested && b.ReturnDate == null && b.BookCopy.AvailableStatus == BookCopyStatus.IssueReturn) ||
                    (status == BorrowingStatus.Overdue && b.ReturnDate == null && b.BookCopy.AvailableStatus != BookCopyStatus.IssueReturn && b.DueDate < now) ||
                    (status == BorrowingStatus.Borrowed && b.ReturnDate == null && b.BookCopy.AvailableStatus != BookCopyStatus.IssueReturn && b.DueDate >= now)) &&
                (isbn != null ? b.BookCopy.Book.Isbn == isbn :
                    search == null || b.BookCopy.Book.Title.Contains(search) || b.User.FirstName.Contains(search) || b.User.LastName.Contains(search));

            int totalCount = await _repository.CountAsync(filter, cancellationToken);
            IEnumerable<Borrowing> borrowings = await _repository.GetAllAsync(filter, _includes, request.PageSize, request.PageNo,
                orderBy: q => q.OrderByDescending(b => b.BorrowDate).ThenBy(b => b.Id), tracked: false, cancellationToken: cancellationToken);

            return Result<PagedResponse<BorrowingResponse>>.Success(new PagedResponse<BorrowingResponse>
            {
                Items = borrowings.Select(b => ToResponse(b, b.BookCopy?.AvailableStatus, now)).ToList(),
                PageNo = request.PageNo,
                PageSize = request.PageSize,
                TotalCount = totalCount
            });
        }

        // End user borrows for themselves.
        public async Task<Result<BorrowingResponse>> Borrow(Guid userId, BorrowRequest request, CancellationToken cancellationToken = default)
        {
            var validateResult = await _borrowValidator.ValidateAsync(request, cancellationToken);
            if (!validateResult.IsValid) return ValidationResult<BorrowingResponse>.From(validateResult.Errors);

            return await CreateBorrowing(userId, request.BookCopyId, cancellationToken);
        }

        // Librarian assigns a copy directly to a user.
        public async Task<Result<BorrowingResponse>> AssignBorrow(AssignBorrowRequest request, CancellationToken cancellationToken = default)
        {
            var validateResult = await _assignValidator.ValidateAsync(request, cancellationToken);
            if (!validateResult.IsValid) return ValidationResult<BorrowingResponse>.From(validateResult.Errors);

            return await CreateBorrowing(request.UserId, request.BookCopyId, cancellationToken);
        }

        // Borrower asks to return: Borrowed -> IssueReturn. The loan stays open until an admin processes it.
        public async Task<Result<BorrowingResponse>> RequestReturn(Guid id, Guid ownerId, CancellationToken cancellationToken = default)
        {
            Borrowing borrowing = await _repository.GetAsync(b => b.Id == id && b.UserId == ownerId, cancellationToken: cancellationToken);
            if (borrowing is null) return Result<BorrowingResponse>.Failure(RepositoryErrors.EntityNotFound);
            if (borrowing.IsReturned) return Result<BorrowingResponse>.Failure(BorrowingErrors.AlreadyReturned);

            BookCopy copy = await _bookCopyRepository.GetAsync(c => c.Id == borrowing.BookCopyId, tracked: true, cancellationToken: cancellationToken);
            if (copy is null) return Result<BorrowingResponse>.Failure(BorrowingServiceErrors.BookCopyNotFound);

            Result requestResult = copy.RequestReturn();
            if (requestResult.IsFailure) return Result<BorrowingResponse>.Failure(requestResult.Error);

            _bookCopyRepository.Update(copy);
            await _bookCopyRepository.CommitAsync(cancellationToken);

            return await GetBorrowing(borrowing.Id, cancellationToken: cancellationToken);
        }

        // Librarian receives the copy: closes the loan and puts the copy back to Available.
        public async Task<Result<BorrowingResponse>> ProcessReturn(Guid id, CancellationToken cancellationToken = default)
        {
            Borrowing borrowing = await _repository.GetAsync(b => b.Id == id, tracked: true, cancellationToken: cancellationToken);
            if (borrowing is null) return Result<BorrowingResponse>.Failure(RepositoryErrors.EntityNotFound);

            BookCopy copy = await _bookCopyRepository.GetAsync(c => c.Id == borrowing.BookCopyId, tracked: true, cancellationToken: cancellationToken);
            if (copy is null) return Result<BorrowingResponse>.Failure(BorrowingServiceErrors.BookCopyNotFound);

            Result returnResult = borrowing.Return(Now);
            if (returnResult.IsFailure) return Result<BorrowingResponse>.Failure(returnResult.Error);

            Result copyReturnResult = copy.Return();
            if (copyReturnResult.IsFailure) return Result<BorrowingResponse>.Failure(copyReturnResult.Error);

            _bookCopyRepository.Update(copy);
            _repository.Update(borrowing);
            await _repository.CommitAsync(cancellationToken);

            return await GetBorrowing(borrowing.Id, cancellationToken: cancellationToken);
        }

        private async Task<Result<BorrowingResponse>> CreateBorrowing(Guid userId, Guid bookCopyId, CancellationToken cancellationToken)
        {
            User user = await _userRepository.GetAsync(u => u.Id == userId, cancellationToken: cancellationToken);
            if (user is null) return Result<BorrowingResponse>.Failure(BorrowingServiceErrors.UserNotFound);
            if (!user.IsActive) return Result<BorrowingResponse>.Failure(BorrowingServiceErrors.UserInactive);

            BookCopy copy = await _bookCopyRepository.GetAsync(c => c.Id == bookCopyId, tracked: true, cancellationToken: cancellationToken);
            if (copy is null) return Result<BorrowingResponse>.Failure(BorrowingServiceErrors.BookCopyNotFound);

            Result borrowCopyResult = copy.Borrow();
            if (borrowCopyResult.IsFailure) return Result<BorrowingResponse>.Failure(borrowCopyResult.Error);

            DateTime now = Now;

            Result<Borrowing> createResult = Borrowing.Create(user.Id, copy.Id, now, now.Add(_loanPeriod));
            if (createResult.IsFailure) return Result<BorrowingResponse>.Failure(createResult.Error);

            Borrowing borrowing = createResult.Value;

            // Copy status and borrowing are saved together; the open-loan unique index guards concurrent borrows.
            await _repository.AddAsync(borrowing, cancellationToken);
            _bookCopyRepository.Update(copy);
            await _repository.CommitAsync(cancellationToken);

            // Reload so the response carries user and book details.
            return await GetBorrowing(borrowing.Id, cancellationToken: cancellationToken);
        }

        private BorrowingResponse ToResponse(Borrowing borrowing, BookCopyStatus? copyStatus, DateTime now) =>
            _mapper.Map<BorrowingResponse>(borrowing) with { Status = GetStatus(borrowing, copyStatus, now) };

        private static BorrowingStatus GetStatus(Borrowing borrowing, BookCopyStatus? copyStatus, DateTime now)
        {
            if (borrowing.IsReturned) return BorrowingStatus.Returned;
            if (copyStatus == BookCopyStatus.IssueReturn) return BorrowingStatus.ReturnRequested;
            if (borrowing.IsOverdue(now)) return BorrowingStatus.Overdue;

            return BorrowingStatus.Borrowed;
        }
    }
}
