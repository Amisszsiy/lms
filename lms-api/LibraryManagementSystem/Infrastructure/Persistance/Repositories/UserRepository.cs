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
    public class UserRepository : Repository<User>, IUserRepository
    {
        public UserRepository(LibraryDbContext context) : base(context)
        {
        }

        public async Task<bool> EmailExists(EmailAddress email, Guid excludeUserId = default, CancellationToken cancellationToken = default)
        {
            return await _context.Users.AnyAsync(u => u.Email == email && u.Id != excludeUserId, cancellationToken);
        }

        public async Task<bool> HasOpenBorrowings(Guid id, CancellationToken cancellationToken = default)
        {
            // IsReturned is computed and not mapped, so query ReturnDate directly.
            return await _context.Borrowings.AnyAsync(b => b.UserId == id && b.ReturnDate == null, cancellationToken);
        }
    }
}
