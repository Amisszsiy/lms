using Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Errors
{
    public static class BookErrors
    {
        public static readonly Error IsbnNotProvided = new Error("Book.IsbnNotProvided", "ISBN is required.");
        public static readonly Error TitleNotProvided = new Error("Book.TitleNotProvided", "Book title is required.");
        public static readonly Error PublisherNotProvided = new Error("Book.PublisherNotProvided", "Publisher is required.");
        public static readonly Error InvalidPublicationYear = new Error("Book.InvalidPublicationYear", "Publication year must be between 1 and the current year.");
        public static readonly Error AuthorAlreadyAdded = new Error("Book.AuthorAlreadyAdded", "Author is already assigned to this book.");
        public static readonly Error AuthorNotFound = new Error("Book.AuthorNotFound", "Author is not assigned to this book.");
        public static readonly Error GenreAlreadyAdded = new Error("Book.GenreAlreadyAdded", "Genre is already assigned to this book.");
        public static readonly Error GenreNotFound = new Error("Book.GenreNotFound", "Genre is not assigned to this book.");
        public static readonly Error HasBorrowedCopies = new Error("Book.HasBorrowedCopies", "Book has copies that are currently borrowed.");
    }
}
