using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.Users
{
    // Used by admin, can also change roles.
    public record UpdateUserRequest : IUserProfileRequest
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Tel { get; set; }
        public List<int> RoleIds { get; set; } = [];
    }
}
