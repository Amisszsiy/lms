using Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Errors
{
    public static class UserServiceErrors
    {
        public static readonly Error EmailAlreadyExists = new Error("User.EmailAlreadyExists", "Another user already uses this email.");
        public static readonly Error UsernameAlreadyExists = new Error("User.UsernameAlreadyExists", "Username is already taken.");
        public static readonly Error RoleNotFound = new Error("User.RoleDoesNotExist", "One or more roles do not exist.");
        public static readonly Error DefaultRoleNotFound = new Error("User.DefaultRoleNotFound", "Default role for new users is not configured.");
        public static readonly Error HasOpenBorrowings = new Error("User.HasOpenBorrowings", "Can't deactivate a user who still has borrowed books.");
        public static readonly Error InvalidCredentials = new Error("User.InvalidCredentials", "Username or password is incorrect.");
        public static readonly Error UserInactive = new Error("User.Inactive", "This account is not active.");
    }
}
