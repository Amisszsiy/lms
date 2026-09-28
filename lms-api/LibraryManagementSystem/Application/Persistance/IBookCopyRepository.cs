using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Persistance
{
    public interface IBookCopyRepository : IRepository<BookCopy>
    {
        /// <summary>
        /// Count Available copies of each book
        /// </summary>
        /// <returns>Book id to count. Books without an available copy are left out</returns>
        Task<Dictionary<long, int>> CountAvailableByBook(List<long> bookIds, CancellationToken cancellationToken = default);
    }
}
