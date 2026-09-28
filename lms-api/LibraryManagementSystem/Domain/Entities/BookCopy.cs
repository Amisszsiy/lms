using Domain.Common;
using Domain.Enums;
using Domain.Errors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class BookCopy : BaseEntity<Guid>, IAuditable
    {
        public long BookId { get; private set; }
        public int LocationId { get; private set; }
        public BookCopyStatus AvailableStatus { get; private set; }

        public Book Book { get; private set; }

        public bool IsOnLoan => AvailableStatus == BookCopyStatus.Borrowed || AvailableStatus == BookCopyStatus.IssueReturn;

        public Guid? CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; }
        public Guid? ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }

        public static Result<BookCopy> Create(long bookId, int locationId)
        {
            if (bookId <= 0) return Result<BookCopy>.Failure(BookCopyErrors.BookNotProvided);
            if (locationId <= 0) return Result<BookCopy>.Failure(BookCopyErrors.LocationNotProvided);

            BookCopy bookCopy = new BookCopy()
            {
                BookId = bookId,
                LocationId = locationId,
                AvailableStatus = BookCopyStatus.Available
            };

            return Result<BookCopy>.Success(bookCopy);
        }

        public Result MoveTo(int locationId)
        {
            if (locationId <= 0) return Result.Failure(BookCopyErrors.LocationNotProvided);

            LocationId = locationId;

            return Result.Success();
        }

        public Result Borrow()
        {
            if (AvailableStatus != BookCopyStatus.Available) return Result.Failure(BookCopyErrors.NotAvailable);

            AvailableStatus = BookCopyStatus.Borrowed;

            return Result.Success();
        }

        public Result RequestReturn()
        {
            if (AvailableStatus == BookCopyStatus.IssueReturn) return Result.Failure(BookCopyErrors.ReturnAlreadyRequested);
            if (AvailableStatus != BookCopyStatus.Borrowed) return Result.Failure(BookCopyErrors.NotBorrowed);

            AvailableStatus = BookCopyStatus.IssueReturn;

            return Result.Success();
        }

        // Also accepts Borrowed so a walk-in return doesn't need a prior request.
        public Result Return()
        {
            if (!IsOnLoan) return Result.Failure(BookCopyErrors.NotBorrowed);

            AvailableStatus = BookCopyStatus.Available;

            return Result.Success();
        }

        public Result MarkDamaged()
        {
            // Borrowed copy must be returned first, otherwise its borrowing can't be closed.
            if (AvailableStatus != BookCopyStatus.Available) return Result.Failure(BookCopyErrors.InvalidStatusTransition);

            AvailableStatus = BookCopyStatus.Damaged;

            return Result.Success();
        }

        public Result MarkLost()
        {
            if (AvailableStatus != BookCopyStatus.Available) return Result.Failure(BookCopyErrors.InvalidStatusTransition);

            AvailableStatus = BookCopyStatus.Lost;

            return Result.Success();
        }

        // Repaired or found copy goes back on the shelf.
        public Result Restore()
        {
            if (AvailableStatus != BookCopyStatus.Damaged && AvailableStatus != BookCopyStatus.Lost) return Result.Failure(BookCopyErrors.InvalidStatusTransition);

            AvailableStatus = BookCopyStatus.Available;

            return Result.Success();
        }

        public void Withdraw() => AvailableStatus = BookCopyStatus.Withdrawn;
    }
}
