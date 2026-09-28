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
    public class AuthorRepository : Repository<Author>, IAuthorRepository
    {
        public AuthorRepository(LibraryDbContext context) : base(context)
        {
        }

        public async Task<bool> AllExist(IReadOnlyCollection<int> ids, CancellationToken cancellationToken = default)
        {
            if (ids is null || ids.Count == 0) return true;

            int count = await _context.Authors.CountAsync(a => ids.Contains(a.Id), cancellationToken);
            return count == ids.Count;
        }

        public async Task<bool> IsUsing(int id, CancellationToken cancellationToken = default)
        {
            return await _context.BookAuthors.AnyAsync(ba => ba.AuthorId == id, cancellationToken);
        }
    }
}
