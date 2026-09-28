using Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class UserRole : BaseEntity<int>
    {
        public Guid UserId { get; private set; }
        public int RoleId { get; private set; }

        internal static UserRole Create(int roleId) => new UserRole() { RoleId = roleId };
    }
}
