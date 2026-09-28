using Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Errors
{
    public static class RoleErrors
    {
        public static readonly Error NameNotProvided = new Error("Role.NameNotProvided", "Role name is required.");
    }
}
