using System.Linq.Expressions;
using Application.Errors;
using Application.Features.Borrowings;
using Application.Persistance;
using AutoMapper;
using Domain.Common;
using Domain.Entities;
using Domain.Enums;
using Domain.Errors;
using Domain.ValueObjects;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Time.Testing;
using Moq;

namespace LibraryManagementSystem.Tests.Application.Borrowings
{
    public class BorrowingServiceTests
    {
        private static readonly DateTimeOffset _now = new DateTimeOffset(2026, 1, 1, 9, 0, 0, TimeSpan.Zero);

        private readonly Mock<IBorrowingRepository> _borrowingRepository = new();
        private readonly Mock<IBookCopyRepository> _bookCopyRepository = new();
        private readonly Mock<IUserRepository> _userRepository = new();
        private readonly FakeTimeProvider _timeProvider = new(_now);
        private readonly BorrowingService _service;

        public BorrowingServiceTests()
        {
            IMapper mapper = new MapperConfiguration(cfg => cfg.AddProfile<BorrowingProfile>(), NullLoggerFactory.Instance).CreateMapper();

            _service = new BorrowingService(
                _borrowingRepository.Object,
                _bookCopyRepository.Object,
                _userRepository.Object,
                mapper,
                new BorrowValidator(),
                new AssignBorrowValidator(),
                new SearchBorrowingsValidator(),
                _timeProvider);
        }

        [Fact]
        public async Task Borrow_AvailableCopyAndActiveUser_CreatesBorrowingAndMarksCopyBorrowed()
        {
            User user = CreateActiveUser();
            BookCopy copy = WithId(BookCopy.Create(1, 1).Value, Guid.NewGuid());
            Borrowing? added = null;

            _userRepository
                .Setup(r => r.GetAsync(It.IsAny<Expression<Func<User, bool>>>(), null, false, It.IsAny<CancellationToken>()))
                .ReturnsAsync(user);
            SetupCopy(copy);
            _borrowingRepository
                .Setup(r => r.AddAsync(It.IsAny<Borrowing>(), It.IsAny<CancellationToken>()))
                .Callback<Borrowing, CancellationToken>((b, _) => added = WithId(b, Guid.NewGuid()));
            _borrowingRepository
                .Setup(r => r.GetAsync(It.IsAny<Expression<Func<Borrowing, bool>>>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(() => added);

            var result = await _service.Borrow(user.Id, new BorrowRequest { BookCopyId = copy.Id });

            Assert.True(result.IsSuccess);
            Assert.Equal(BorrowingStatus.Borrowed, result.Value.Status);
            Assert.Equal(user.Id, result.Value.UserId);
            Assert.Equal(copy.Id, result.Value.BookCopyId);
            Assert.Equal(_now.UtcDateTime, result.Value.BorrowDate);
            Assert.Equal(_now.UtcDateTime.AddDays(14), result.Value.DueDate);
            Assert.Null(result.Value.ReturnDate);

            Assert.Equal(BookCopyStatus.Borrowed, copy.AvailableStatus);
            Assert.NotNull(added);
            _bookCopyRepository.Verify(r => r.Update(copy), Times.Once);
            _borrowingRepository.Verify(r => r.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task AssignBorrow_MissingUser_ReturnsValidationError()
        {
            var result = await _service.AssignBorrow(new AssignBorrowRequest { BookCopyId = Guid.NewGuid() });

            Assert.True(result.IsFailure);
            _borrowingRepository.Verify(r => r.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task RequestReturn_ByOwner_MarksCopyIssueReturnAndKeepsLoanOpen()
        {
            Guid ownerId = Guid.NewGuid();
            BookCopy copy = BorrowedCopy();
            Borrowing borrowing = CreateBorrowing(ownerId, copy);

            SetupBorrowing(borrowing);
            SetupCopy(copy);

            var result = await _service.RequestReturn(borrowing.Id, ownerId);

            Assert.True(result.IsSuccess);
            Assert.Equal(BorrowingStatus.ReturnRequested, result.Value.Status);
            Assert.Null(result.Value.ReturnDate);

            Assert.False(borrowing.IsReturned);
            Assert.Equal(BookCopyStatus.IssueReturn, copy.AvailableStatus);
            _bookCopyRepository.Verify(r => r.Update(copy), Times.Once);
            _bookCopyRepository.Verify(r => r.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task RequestReturn_ByAnotherUser_ReturnsNotFound()
        {
            BookCopy copy = BorrowedCopy();
            Borrowing borrowing = CreateBorrowing(Guid.NewGuid(), copy);

            SetupBorrowing(borrowing);
            SetupCopy(copy);

            var result = await _service.RequestReturn(borrowing.Id, Guid.NewGuid());

            Assert.True(result.IsFailure);
            Assert.Equal(RepositoryErrors.EntityNotFound, result.Error);
            Assert.Equal(BookCopyStatus.Borrowed, copy.AvailableStatus);
        }

        [Fact]
        public async Task RequestReturn_AlreadyRequested_ReturnsError()
        {
            Guid ownerId = Guid.NewGuid();
            BookCopy copy = BorrowedCopy();
            copy.RequestReturn();
            Borrowing borrowing = CreateBorrowing(ownerId, copy);

            SetupBorrowing(borrowing);
            SetupCopy(copy);

            var result = await _service.RequestReturn(borrowing.Id, ownerId);

            Assert.True(result.IsFailure);
            Assert.Equal(BookCopyErrors.ReturnAlreadyRequested, result.Error);
        }

        [Fact]
        public async Task ProcessReturn_ReturnRequested_ClosesBorrowingAndMarksCopyAvailable()
        {
            BookCopy copy = BorrowedCopy();
            copy.RequestReturn();
            Borrowing borrowing = CreateBorrowing(Guid.NewGuid(), copy);

            SetupBorrowing(borrowing);
            SetupCopy(copy);

            _timeProvider.Advance(TimeSpan.FromDays(3));

            var result = await _service.ProcessReturn(borrowing.Id);

            Assert.True(result.IsSuccess);
            Assert.Equal(BorrowingStatus.Returned, result.Value.Status);
            Assert.Equal(_now.UtcDateTime.AddDays(3), result.Value.ReturnDate);

            Assert.True(borrowing.IsReturned);
            Assert.Equal(BookCopyStatus.Available, copy.AvailableStatus);
            _bookCopyRepository.Verify(r => r.Update(copy), Times.Once);
            _borrowingRepository.Verify(r => r.Update(borrowing), Times.Once);
            _borrowingRepository.Verify(r => r.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task ProcessReturn_WithoutRequest_ClosesBorrowing()
        {
            BookCopy copy = BorrowedCopy();
            Borrowing borrowing = CreateBorrowing(Guid.NewGuid(), copy);

            SetupBorrowing(borrowing);
            SetupCopy(copy);

            var result = await _service.ProcessReturn(borrowing.Id);

            Assert.True(result.IsSuccess);
            Assert.Equal(BorrowingStatus.Returned, result.Value.Status);
            Assert.Equal(BookCopyStatus.Available, copy.AvailableStatus);
        }

        [Fact]
        public async Task ProcessReturn_AlreadyReturned_ReturnsError()
        {
            BookCopy copy = BorrowedCopy();
            Borrowing borrowing = CreateBorrowing(Guid.NewGuid(), copy);
            borrowing.Return(_now.UtcDateTime);

            SetupBorrowing(borrowing);
            SetupCopy(copy);

            var result = await _service.ProcessReturn(borrowing.Id);

            Assert.True(result.IsFailure);
            Assert.Equal(BorrowingErrors.AlreadyReturned, result.Error);
            _borrowingRepository.Verify(r => r.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        // Evaluates the service's filter so ownership checks are exercised.
        private void SetupBorrowing(Borrowing borrowing)
        {
            _borrowingRepository
                .Setup(r => r.GetAsync(It.IsAny<Expression<Func<Borrowing, bool>>>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((Expression<Func<Borrowing, bool>> filter, string _, bool _, CancellationToken _) => filter.Compile()(borrowing) ? borrowing : null);
        }

        private void SetupCopy(BookCopy copy)
        {
            _bookCopyRepository
                .Setup(r => r.GetAsync(It.IsAny<Expression<Func<BookCopy, bool>>>(), null, true, It.IsAny<CancellationToken>()))
                .ReturnsAsync(copy);
        }

        private static BookCopy BorrowedCopy()
        {
            BookCopy copy = WithId(BookCopy.Create(1, 1).Value, Guid.NewGuid());
            copy.Borrow();

            return copy;
        }

        // BookCopy navigation is normally loaded by the repository include.
        private static Borrowing CreateBorrowing(Guid userId, BookCopy copy)
        {
            Borrowing borrowing = WithId(Borrowing.Create(userId, copy.Id, _now.UtcDateTime, _now.UtcDateTime.AddDays(14)).Value, Guid.NewGuid());
            typeof(Borrowing).GetProperty(nameof(Borrowing.BookCopy))!.SetValue(borrowing, copy);

            return borrowing;
        }

        private static User CreateActiveUser()
        {
            User user = WithId(User.Create("John", "Doe", EmailAddress.Create("john.doe@example.com").Value, "0800000000").Value, Guid.NewGuid());
            user.SetActive(true);

            return user;
        }

        // Id is normally generated by the database and has a protected setter.
        private static T WithId<T>(T entity, Guid id) where T : BaseEntity<Guid>
        {
            typeof(BaseEntity<Guid>).GetProperty(nameof(BaseEntity<Guid>.Id))!.SetValue(entity, id);

            return entity;
        }
    }
}
