using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Persistance
{
    public interface IRoleRepository : IRepository<Role>
    {
        /// <summary>
        /// Check if every given role exists
        /// </summary>
        /// <param name="ids">Distinct role ids</param>
        /// <returns>True if all roles exist</returns>
        Task<bool> AllExist(IReadOnlyCollection<int> ids, CancellationToken cancellationToken = default);

        /// <summary>
        /// Get role id by role name
        /// </summary>
        /// <returns>Role id, null if role doesn't exist</returns>
        Task<int?> GetIdByName(string roleName, CancellationToken cancellationToken = default);

        /// <summary>
        /// Get role names by role ids
        /// </summary>
        /// <returns>Names of the roles that exist</returns>
        Task<List<string>> GetNamesByIds(IReadOnlyCollection<int> ids, CancellationToken cancellationToken = default);
    }
}
