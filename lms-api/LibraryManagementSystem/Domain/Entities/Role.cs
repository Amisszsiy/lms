using Domain.Common;
using Domain.Errors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class Role : BaseEntity<int>
    {
        public string RoleName { get; private set; }

        public static Result<Role> Create(string roleName)
        {
            if (string.IsNullOrWhiteSpace(roleName)) return Result<Role>.Failure(RoleErrors.NameNotProvided);

            return Result<Role>.Success(new Role() { RoleName = roleName });
        }
    }
}
