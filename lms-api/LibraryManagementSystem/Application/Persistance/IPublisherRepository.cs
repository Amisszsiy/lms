using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Persistance
{
    public interface IPublisherRepository : IRepository<Publisher>
    {
        /// <summary>
        /// Check if publisher is being referenced by any book
        /// </summary>
        /// <param name="Id"></param>
        /// <returns>True if being referenced</returns>
        Task<bool> IsUsing(int Id);

        /// <summary>
        /// Check if the publisher exists
        /// </summary>
        /// <returns>True if publisher exists</returns>
        Task<bool> Exists(int id, CancellationToken cancellationToken = default);
    }
}
