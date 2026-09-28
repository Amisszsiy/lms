using Application.Persistance;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Persistance.Repositories
{
    public class RoleRepository : Repository<Role>, IRoleRepository
    {
        public RoleRepository(LibraryDbContext context) : base(context)
        {
        }

        public async Task<bool> AllExist(IReadOnlyCollection<int> ids, CancellationToken cancellationToken = default)
        {
            if (ids is null || ids.Count == 0) return true;

            int count = await _context.Roles.CountAsync(r => ids.Contains(r.Id), cancellationToken);
            return count == ids.Count;
        }

        public async Task<int?> GetIdByName(string roleName, CancellationToken cancellationToken = default)
        {
            return await _context.Roles
                .Where(r => r.RoleName == roleName)
                .Select(r => (int?)r.Id)
                .FirstOrDefaultAsync(cancellationToken);
        }

        public async Task<List<string>> GetNamesByIds(IReadOnlyCollection<int> ids, CancellationToken cancellationToken = default)
        {
            if (ids is null || ids.Count == 0) return new List<string>();

            return await _context.Roles
                .Where(r => ids.Contains(r.Id))
                .Select(r => r.RoleName)
                .ToListAsync(cancellationToken);
        }
    }
}
