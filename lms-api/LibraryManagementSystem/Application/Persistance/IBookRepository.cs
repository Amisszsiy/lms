using Domain.Entities;
using Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Persistance
{
    public interface IBookRepository : IRepository<Book>
    {
        /// <summary>
        /// Check if another book already uses this ISBN
        /// </summary>
        /// <param name="isbn"></param>
        /// <param name="excludeBookId">Book to ignore, used when updating</param>
        /// <returns>True if ISBN is taken</returns>
        Task<bool> IsbnExists(Isbn isbn, long excludeBookId = 0, CancellationToken cancellationToken = default);

        /// <summary>
        /// Check if the book is active
        /// </summary>
        /// <returns>True if active, false if inactive, null if book doesn't exist</returns>
        Task<bool?> IsActive(long id, CancellationToken cancellationToken = default);
    }
}
