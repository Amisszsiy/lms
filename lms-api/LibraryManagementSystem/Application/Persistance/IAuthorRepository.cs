using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Persistance
{
    public interface IAuthorRepository : IRepository<Author>
    {
        /// <summary>
        /// Check if author is being referenced by any book
        /// </summary>
        /// <param name="id"></param>
        /// <returns>True if being referenced</returns>
        Task<bool> IsUsing(int id, CancellationToken cancellationToken = default);

        /// <summary>
        /// Check if every given author exists
        /// </summary>
        /// <param name="ids">Distinct author ids</param>
        /// <returns>True if all authors exist</returns>
        Task<bool> AllExist(IReadOnlyCollection<int> ids, CancellationToken cancellationToken = default);
    }
}
