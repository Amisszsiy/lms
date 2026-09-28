using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.Users
{
    public interface IUserProfileRequest
    {
        string FirstName { get; }
        string LastName { get; }
        string Email { get; }
        string Tel { get; }
    }
}
