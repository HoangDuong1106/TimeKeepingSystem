using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace DataAccess
{
    public class Repository<T> : IRepository<T> where T : class
    {
        private readonly MyDbContext _context;
        private readonly DbSet<T> _dbSet;

        public Repository(MyDbContext context)
        {
            _context = context;
            _dbSet = context.Set<T>();
        }

        public async Task<IQueryable<T>> GetAllAsync(Expression<Func<T, bool>>? filter = null)
        {
            // Check if the IsDeleted property exists using reflection
            var propertyInfo = typeof(T).GetProperty("IsDeleted");
            Expression<Func<T, bool>> isDeletedExpr = null;

            if (propertyInfo != null)
            {
                // Build an expression for the IsDeleted property
                var parameterExp = Expression.Parameter(typeof(T), "type");
                var propertyExp = Expression.Property(parameterExp, "IsDeleted");
                var constantExp = Expression.Constant(false);
                var equalityExp = Expression.Equal(propertyExp, constantExp);

                isDeletedExpr = Expression.Lambda<Func<T, bool>>(equalityExp, parameterExp);
            }

            var result = isDeletedExpr != null ? _dbSet.Where(isDeletedExpr) : _dbSet;

            return filter != null ? result.Where(filter) : result;
        }

        //public async Task<T> GetByIdAsync(Guid id)
        //{
        //    return await _dbSet.FindAsync(id);
        //}

        public async Task<T> AddAsync(T entity)
        {
            ((dynamic)entity).IsDeleted = false;
            await _dbSet.AddAsync(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        //public async Task<T> UpdateAsync(T entity)
        //{
        //    _dbSet.Update(entity);
        //    await _context.SaveChangesAsync();
        //    return entity;
        //}

        //public async Task<T> DeleteAsync(Guid id)
        //{
        //    var entity = await GetByIdAsync(id);
        //    if (entity == null) return null;

        //    _dbSet.Remove(entity);
        //    await _context.SaveChangesAsync();
        //    return entity;
        //}

        public async Task<bool> SoftDeleteAsync(Guid id) // New method for soft deletion
        {
            var entity = await _dbSet.FindAsync(id);
            if (entity == null) return false;

            // Assuming there's an 'IsDeleted' property in your entity
            // You'll have to cast to the concrete type of the entity
            ((dynamic)entity).IsDeleted = true;

            _dbSet.Update(entity);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
