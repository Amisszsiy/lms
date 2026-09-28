using Domain.Common;
using Domain.Errors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class Borrowing : BaseEntity<Guid>, IAuditable
    {
        public Guid UserId { get; private set; }
        public Guid BookCopyId { get; private set; }
        public DateTime BorrowDate { get; private set; }
        public DateTime? ReturnDate { get; private set; }
        public DateTime DueDate { get; private set; }

        public User User { get; private set; }
        public BookCopy BookCopy { get; private set; }

        public bool IsReturned => ReturnDate.HasValue;

        public Guid? CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; }
        public Guid? ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }

        public static Result<Borrowing> Create(Guid userId, Guid bookCopyId, DateTime borrowDate, DateTime dueDate)
        {
            if (userId == Guid.Empty) return Result<Borrowing>.Failure(BorrowingErrors.UserNotProvided);
            if (bookCopyId == Guid.Empty) return Result<Borrowing>.Failure(BorrowingErrors.BookCopyNotProvided);
            if (dueDate <= borrowDate) return Result<Borrowing>.Failure(BorrowingErrors.DueDateBeforeBorrowDate);

            Borrowing borrowTransaction = new Borrowing()
            {
                UserId = userId,
                BookCopyId = bookCopyId,
                BorrowDate = borrowDate,
                DueDate = dueDate
            };

            return Result<Borrowing>.Success(borrowTransaction);
        }

        public Result Return(DateTime returnDate)
        {
            if (IsReturned) return Result.Failure(BorrowingErrors.AlreadyReturned);
            if (returnDate < BorrowDate) return Result.Failure(BorrowingErrors.ReturnDateBeforeBorrowDate);

            ReturnDate = returnDate;

            return Result.Success();
        }

        public bool IsOverdue(DateTime now) => !IsReturned && now > DueDate;
    }
}
