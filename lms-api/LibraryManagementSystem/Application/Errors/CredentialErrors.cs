using Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Errors
{
    public static class CredentialErrors
    {
        public static readonly Error UsernameNotProvided = new Error("Credential.UsernameNotProvided", "Username is required.");
        public static readonly Error PasswordNotProvided = new Error("Credential.PasswordNotProvided", "Password is required.");
        public static readonly Error UserNotProvided = new Error("Credential.UserNotProvided", "Credential must belong to a user.");
    }
}
