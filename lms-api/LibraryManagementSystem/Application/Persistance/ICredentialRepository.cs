using Application.Features.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Persistance
{
    public interface ICredentialRepository : IRepository<Credential>
    {
        /// <summary>
        /// Check if the username is already taken
        /// </summary>
        /// <returns>True if username is taken</returns>
        Task<bool> UsernameExists(string username, CancellationToken cancellationToken = default);
    }
}
