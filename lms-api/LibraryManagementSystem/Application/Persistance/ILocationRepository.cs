using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Persistance
{
    public interface ILocationRepository : IRepository<Location>
    {
        /// <summary>
        /// Check if the location exists
        /// </summary>
        /// <returns>True if location exists</returns>
        Task<bool> Exists(int id, CancellationToken cancellationToken = default);

        /// <summary>
        /// Check if location is being referenced by any book copy
        /// </summary>
        /// <returns>True if being referenced</returns>
        Task<bool> IsUsing(int id, CancellationToken cancellationToken = default);

        /// <summary>
        /// Check if another location already occupies this position
        /// </summary>
        /// <param name="excludeLocationId">Location to ignore, used when updating</param>
        /// <returns>True if position is taken</returns>
        Task<bool> PositionExists(string area, string lineNo, string shelfNo, int rowNo, int columnNo, int excludeLocationId = 0, CancellationToken cancellationToken = default);
    }
}
