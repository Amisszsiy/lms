using Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Errors
{
    public static class BorrowingErrors
    {
        public static readonly Error UserNotProvided = new Error("BorrowTransaction.UserNotProvided", "User is required.");
        public static readonly Error BookCopyNotProvided = new Error("BorrowTransaction.BookCopyNotProvided", "Book copy is required.");
        public static readonly Error DueDateBeforeBorrowDate = new Error("BorrowTransaction.DueDateBeforeBorrowDate", "Due date must be after the borrow date.");
        public static readonly Error AlreadyReturned = new Error("BorrowTransaction.AlreadyReturned", "Book copy has already been returned.");
        public static readonly Error ReturnDateBeforeBorrowDate = new Error("BorrowTransaction.ReturnDateBeforeBorrowDate", "Return date cannot be before the borrow date.");
    }
}
