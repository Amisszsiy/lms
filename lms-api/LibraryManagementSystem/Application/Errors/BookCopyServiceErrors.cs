using Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Errors
{
    public static class BookCopyServiceErrors
    {
        public static readonly Error BookNotFound = new Error("BookCopy.BookNotFound", "Book does not exist.");
        public static readonly Error BookInactive = new Error("BookCopy.BookInactive", "Can't add copies to an inactive book.");
        public static readonly Error LocationNotFound = new Error("BookCopy.LocationNotFound", "Location does not exist.");
    }
}
