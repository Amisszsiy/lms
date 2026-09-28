using Application.Persistance;
using Domain.Entities;
using Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Persistance.Repositories
{
    public class BookRepository : Repository<Book>, IBookRepository
    {
        public BookRepository(LibraryDbContext context) : base(context)
        {
        }

        public async Task<bool> IsbnExists(Isbn isbn, long excludeBookId = 0, CancellationToken cancellationToken = default)
        {
            return await _context.Books.AnyAsync(b => b.Isbn == isbn && b.Id != excludeBookId, cancellationToken);
        }

        public async Task<bool?> IsActive(long id, CancellationToken cancellationToken = default)
        {
            return await _context.Books
                .Where(b => b.Id == id)
                .Select(b => (bool?)b.IsActive)
                .FirstOrDefaultAsync(cancellationToken);
        }
    }
}
