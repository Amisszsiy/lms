using Domain.Entities;
using Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Persistance
{
    public interface IUserRepository : IRepository<User>
    {
        /// <summary>
        /// Check if another user already uses this email
        /// </summary>
        /// <param name="email"></param>
        /// <param name="excludeUserId">User to ignore, used when updating</param>
        /// <returns>True if email is taken</returns>
        Task<bool> EmailExists(EmailAddress email, Guid excludeUserId = default, CancellationToken cancellationToken = default);

        /// <summary>
        /// Check if the user has any borrowing that is not returned yet
        /// </summary>
        /// <returns>True if user has open borrowings</returns>
        Task<bool> HasOpenBorrowings(Guid id, CancellationToken cancellationToken = default);
    }
}
