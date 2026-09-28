using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Application.Persistance
{
    public interface IRepository<T> where T : class
    {
        //pageNo starts at 1. pageSize 0 returns everything
        IEnumerable<T> GetAll(Expression<Func<T, bool>> filter = null, string includeProperties = null, int pageSize = 0, int pageNo = 1);
        //No tracking only this method to show an example of configuration
        T Get(Expression<Func<T, bool>> filter, string includeProperties = null, bool tracked = false);
        T Get(Guid id);
        T Get(int id);
        void Add(T entity);
        void Update(T entity);
        void UpdateRange(List<T> entities);
        void Remove(T entity);
        void RemoveRange(IEnumerable<T> entities);
        void Commit();

        //Pass orderBy whenever pageSize > 0, otherwise the pages come back in no fixed order
        Task<IEnumerable<T>> GetAllAsync(Expression<Func<T, bool>> filter = null, string includeProperties = null, int pageSize = 0, int pageNo = 1,
            Func<IQueryable<T>, IOrderedQueryable<T>> orderBy = null, bool tracked = true, CancellationToken cancellationToken = default);
        Task<int> CountAsync(Expression<Func<T, bool>> filter = null, CancellationToken cancellationToken = default);
        Task<T> GetAsync(Expression<Func<T, bool>> filter, string includeProperties = null, bool tracked = false, CancellationToken cancellationToken = default);
        Task<T> GetAsync(Guid id);
        Task<T> GetAsync(int id);
        Task AddAsync(T entity, CancellationToken cancellationToken = default);

        Task CommitAsync(CancellationToken cancellationToken = default);
    }
}
