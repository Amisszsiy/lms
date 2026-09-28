using Application.Features.Users;
using Application.Persistance;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Persistance.Repositories
{
    public class CredentialRepository : Repository<Credential>, ICredentialRepository
    {
        public CredentialRepository(LibraryDbContext context) : base(context)
        {
        }

        public async Task<bool> UsernameExists(string username, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(username)) return false;

            // Credential.Create stores the trimmed username, so compare the same way.
            string trimmed = username.Trim();
            return await _context.Credentials.AnyAsync(c => c.Username == trimmed, cancellationToken);
        }
    }
}
