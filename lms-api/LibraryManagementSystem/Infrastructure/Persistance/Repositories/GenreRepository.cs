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
    public class GenreRepository : Repository<Genre>, IGenreRepository
    {
        public GenreRepository(LibraryDbContext context) : base(context)
        {
        }

        public async Task<bool> AllExist(IReadOnlyCollection<int> ids, CancellationToken cancellationToken = default)
        {
            if (ids is null || ids.Count == 0) return true;

            int count = await _context.Genres.CountAsync(g => ids.Contains(g.Id), cancellationToken);
            return count == ids.Count;
        }

        public async Task<bool> IsUsing(int id, CancellationToken cancellationToken = default)
        {
            return await _context.BookGenres.AnyAsync(bg => bg.GenreId == id, cancellationToken);
        }

        public async Task<bool> NameExists(string genreName, int excludeGenreId = 0, CancellationToken cancellationToken = default)
        {
            return await _context.Genres.AnyAsync(g => g.GenreName == genreName && g.Id != excludeGenreId, cancellationToken);
        }
    }
}
