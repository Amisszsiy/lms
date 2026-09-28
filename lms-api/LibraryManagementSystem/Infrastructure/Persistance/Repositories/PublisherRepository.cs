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
    public class PublisherRepository : Repository<Publisher>, IPublisherRepository
    {
        public PublisherRepository(LibraryDbContext context) : base(context)
        {
        }

        public async Task<bool> IsUsing(int Id)
        {
            return await _context.Books.AnyAsync(b => b.PublisherId == Id);
        }

        public async Task<bool> Exists(int id, CancellationToken cancellationToken = default)
        {
            return await _context.Publishers.AnyAsync(p => p.Id == id, cancellationToken);
        }
    }
}
