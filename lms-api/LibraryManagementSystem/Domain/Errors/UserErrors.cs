using Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Errors
{
    public static class UserErrors
    {
        public static readonly Error NameNotProvided = new Error("User.NameNotProvided", "First name and last name are required.");
        public static readonly Error EmailNotProvided = new Error("User.EmailNotProvided", "Email address is required.");
        public static readonly Error RoleAlreadyAssigned = new Error("User.RoleAlreadyAssigned", "Role is already assigned to this user.");
        public static readonly Error RoleNotAssigned = new Error("User.RoleNotAssigned", "Role is not assigned to this user.");
    }
}
