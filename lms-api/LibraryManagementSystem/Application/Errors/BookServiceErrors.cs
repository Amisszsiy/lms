using Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Errors
{
    public static class BookServiceErrors
    {
        public static readonly Error IsbnAlreadyExists = new Error("Book.IsbnAlreadyExists", "Another book already uses this ISBN.");
        public static readonly Error PublisherNotFound = new Error("Book.PublisherNotFound", "Publisher does not exist.");
        public static readonly Error AuthorNotFound = new Error("Book.AuthorDoesNotExist", "One or more authors do not exist.");
        public static readonly Error GenreNotFound = new Error("Book.GenreDoesNotExist", "One or more genres do not exist.");
    }
}
