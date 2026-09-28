using Application.Persistance;
using Domain.Entities;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Persistance.Repositories
{
    public class BookCopyRepository : Repository<BookCopy>, IBookCopyRepository
    {
        public BookCopyRepository(LibraryDbContext context) : base(context)
        {

        }

        public async Task<Dictionary<long, int>> CountAvailableByBook(List<long> bookIds, CancellationToken cancellationToken = default)
        {
            return await _context.BookCopies
                .Where(c => bookIds.Contains(c.BookId) && c.AvailableStatus == BookCopyStatus.Available)
                .GroupBy(c => c.BookId)
                .Select(g => new { BookId = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.BookId, x => x.Count, cancellationToken);
        }
    }
}
