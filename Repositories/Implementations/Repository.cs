using Microsoft.EntityFrameworkCore;
using StockifyPlus.Data;
using StockifyPlus.Repositories.Interfaces;
using System.Linq.Expressions;

namespace StockifyPlus.Repositories.Implementations
{
    public class Repository<T> : IRepository<T> where T : class
    {
        private readonly ApplicationDbContext _context;
        private readonly DbSet<T> _dbSet;

        public Repository(ApplicationDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _dbSet = context.Set<T>();
        }


        public async Task<IEnumerable<T>> GetAllAsync()
        {
            return await _dbSet.ToListAsync();
        }

        public async Task<T> GetByIdAsync(int id)
        {
            return await _dbSet.FindAsync(id);
        }

        public async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate)
        {
            return await _dbSet.Where(predicate).ToListAsync();
        }

        public async Task<T> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate)
        {
            return await _dbSet.FirstOrDefaultAsync(predicate);
        }

        public async Task<int> CountAsync(Expression<Func<T, bool>> predicate = null)
        {
            if (predicate == null)
                return await _dbSet.CountAsync();

            return await _dbSet.CountAsync(predicate);
        }

        public async Task<bool> AnyAsync(Expression<Func<T, bool>> predicate)
        {
            return await _dbSet.AnyAsync(predicate);
        }

        public async Task AddAsync(T entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            await _dbSet.AddAsync(entity);
        }

        public async Task AddRangeAsync(IEnumerable<T> entities)
        {
            if (entities == null || !entities.Any())
                throw new ArgumentException("Entities collection cannot be null or empty.", nameof(entities));

            await _dbSet.AddRangeAsync(entities);
        }

        public void Update(T entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            _dbSet.Update(entity);
        }

        public void UpdateRange(IEnumerable<T> entities)
        {
            if (entities == null || !entities.Any())
                throw new ArgumentException("Entities collection cannot be null or empty.", nameof(entities));

            _dbSet.UpdateRange(entities);
        }


        public void Delete(T entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            _dbSet.Remove(entity);
        }

        public void DeleteRange(IEnumerable<T> entities)
        {
            if (entities == null || !entities.Any())
                throw new ArgumentException("Entities collection cannot be null or empty.", nameof(entities));

            _dbSet.RemoveRange(entities);
        }

        public async Task DeleteByIdAsync(int id)
        {
            var entity = await GetByIdAsync(id);
            if (entity != null)
                Delete(entity);
        }

        public IQueryable<T> IncludeProperties(params Expression<Func<T, object>>[] includeProperties)
        {
            IQueryable<T> query = _dbSet;

            if (includeProperties != null && includeProperties.Length > 0)
            {
                foreach (var includeProperty in includeProperties)
                {
                    query = query.Include(includeProperty);
                }
            }

            return query;
        }
    }
}
