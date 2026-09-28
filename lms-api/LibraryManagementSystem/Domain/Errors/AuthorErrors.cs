using Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Errors
{
    public static class AuthorErrors
    {
        public static readonly Error FirstNameNotProvided = new Error("Author.FirstNameNotProvided", "Author first name is required.");
        public static readonly Error PenNameNotProvided = new Error("Author.PenNameNotProvided", "Pen name is required.");
        public static readonly Error PenNameAlreadyExists = new Error("Author.PenNameAlreadyExists", "Author already has this pen name.");
        public static readonly Error PenNameNotFound = new Error("Author.PenNameNotFound", "Author does not have this pen name.");
    }
}
