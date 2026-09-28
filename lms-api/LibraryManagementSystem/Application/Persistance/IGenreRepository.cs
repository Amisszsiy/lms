using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Persistance
{
    public interface IGenreRepository : IRepository<Genre>
    {
        /// <summary>
        /// Check if every given genre exists
        /// </summary>
        /// <param name="ids">Distinct genre ids</param>
        /// <returns>True if all genres exist</returns>
        Task<bool> AllExist(IReadOnlyCollection<int> ids, CancellationToken cancellationToken = default);

        /// <summary>
        /// Check if genre is being referenced by any book
        /// </summary>
        /// <returns>True if being referenced</returns>
        Task<bool> IsUsing(int id, CancellationToken cancellationToken = default);

        /// <summary>
        /// Check if another genre already uses this name
        /// </summary>
        /// <param name="excludeGenreId">Genre to ignore, used when updating</param>
        /// <returns>True if name is taken</returns>
        Task<bool> NameExists(string genreName, int excludeGenreId = 0, CancellationToken cancellationToken = default);
    }
}
