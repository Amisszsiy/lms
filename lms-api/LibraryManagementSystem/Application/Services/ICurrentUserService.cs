using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services
{
    public interface ICurrentUserService
    {
        // Null when there is no authenticated user (e.g. seeding, anonymous endpoints).
        Guid? UserId { get; }
    }
}
