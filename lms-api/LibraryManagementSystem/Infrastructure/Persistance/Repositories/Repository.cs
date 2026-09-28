using Application.Persistance;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Persistance.Repositories
{
    public class Repository<T> : IRepository<T> where T : class
    {
        protected readonly LibraryDbContext _context;
        protected readonly DbSet<T> _dbSet;

        public Repository(LibraryDbContext context)
        {
            _context = context;
            _dbSet = context.Set<T>();
        }

        public void Add(T entity)
        {
            _dbSet.Add(entity);
        }

        public async Task AddAsync(T entity, CancellationToken cancellationToken = default)
        {
            await _dbSet.AddAsync(entity, cancellationToken);
        }

        public void Commit()
        {
            _context.SaveChanges();
        }

        public async Task CommitAsync(CancellationToken cancellationToken = default)
        {
            await _context.SaveChangesAsync(cancellationToken);
        }

        public T Get(Expression<Func<T, bool>> filter, string includeProperties = null, bool tracked = false)
        {
            return BuildQuery(filter, includeProperties, tracked).FirstOrDefault();
        }

        public T Get(Guid id)
        {
            return _dbSet.Find(id);
        }

        public T Get(int id)
        {
            return _dbSet.Find(id);
        }

        public IEnumerable<T> GetAll(Expression<Func<T, bool>> filter = null, string includeProperties = null, int pageSize = 0, int pageNo = 1)
        {
            return Paginate(BuildQuery(filter, includeProperties, tracked: true), pageSize, pageNo).ToList();
        }

        public async Task<IEnumerable<T>> GetAllAsync(Expression<Func<T, bool>> filter = null, string includeProperties = null, int pageSize = 0, int pageNo = 1,
            Func<IQueryable<T>, IOrderedQueryable<T>> orderBy = null, bool tracked = true, CancellationToken cancellationToken = default)
        {
            IQueryable<T> query = BuildQuery(filter, includeProperties, tracked);
            if (orderBy != null)
            {
                query = orderBy(query);
            }

            return await Paginate(query, pageSize, pageNo).ToListAsync(cancellationToken);
        }

        public async Task<int> CountAsync(Expression<Func<T, bool>> filter = null, CancellationToken cancellationToken = default)
        {
            return await BuildQuery(filter, includeProperties: null, tracked: false).CountAsync(cancellationToken);
        }

        public async Task<T> GetAsync(Expression<Func<T, bool>> filter, string includeProperties = null, bool tracked = false, CancellationToken cancellationToken = default)
        {
            return await BuildQuery(filter, includeProperties, tracked).FirstOrDefaultAsync(cancellationToken);
        }

        public async Task<T> GetAsync(Guid id)
        {
            return await _dbSet.FindAsync(id);
        }

        public async Task<T> GetAsync(int id)
        {
            return await _dbSet.FindAsync(id);
        }

        public void Remove(T entity)
        {
            _dbSet.Remove(entity);
        }

        public void RemoveRange(IEnumerable<T> entities)
        {
            _dbSet.RemoveRange(entities);
        }

        public void Update(T entity)
        {
            _dbSet.Update(entity);
        }

        public void UpdateRange(List<T> entities)
        {
            _dbSet.UpdateRange(entities);
        }

        protected IQueryable<T> BuildQuery(Expression<Func<T, bool>> filter, string includeProperties, bool tracked)
        {
            IQueryable<T> query = tracked ? _dbSet : _dbSet.AsNoTracking();

            if (filter != null)
            {
                query = query.Where(filter);
            }

            if (!string.IsNullOrWhiteSpace(includeProperties))
            {
                foreach (var includeProperty in includeProperties.Split(',', StringSplitOptions.RemoveEmptyEntries))
                {
                    query = query.Include(includeProperty);
                }
            }

            return query;
        }

        protected static IQueryable<T> Paginate(IQueryable<T> query, int pageSize, int pageNo)
        {
            if (pageSize <= 0)
            {
                return query;
            }

            if (pageNo < 1)
            {
                pageNo = 1;
            }

            return query.Skip((pageNo - 1) * pageSize).Take(pageSize);
        }
    }
}
