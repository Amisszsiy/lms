using Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Errors
{
    public static class BookCopyErrors
    {
        public static readonly Error BookNotProvided = new Error("BookCopy.BookNotProvided", "Book is required.");
        public static readonly Error LocationNotProvided = new Error("BookCopy.LocationNotProvided", "Location is required.");
        public static readonly Error NotAvailable = new Error("BookCopy.NotAvailable", "Book copy is not available for borrowing.");
        public static readonly Error NotBorrowed = new Error("BookCopy.NotBorrowed", "Book copy is not currently borrowed.");
        public static readonly Error ReturnAlreadyRequested = new Error("BookCopy.ReturnAlreadyRequested", "Return has already been requested for this book copy.");
        public static readonly Error InvalidStatusTransition = new Error("BookCopy.InvalidStatusTransition", "Book copy status cannot be changed from its current status.");
    }
}
