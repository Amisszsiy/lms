using Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Errors
{
    public static class BorrowingServiceErrors
    {
        public static readonly Error UserNotFound = new Error("Borrowing.UserNotFound", "User does not exist.");
        public static readonly Error UserInactive = new Error("Borrowing.UserInactive", "Inactive users can't borrow books.");
        public static readonly Error BookCopyNotFound = new Error("Borrowing.BookCopyNotFound", "Book copy does not exist.");
    }
}
