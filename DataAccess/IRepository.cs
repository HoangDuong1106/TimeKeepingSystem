using System.Linq.Expressions;

namespace DataAccess
{
    public interface IRepository<T> where T : class
    {
        Task<IQueryable<T>> GetAllAsync(Expression<Func<T, bool>>? filter = null);
        //Task<T> GetByIdAsync(Guid id);
        Task<T> AddAsync(T entity);
        //Task<T> UpdateAsync(T entity);
        //Task<T> DeleteAsync(Guid id);
        Task<bool> SoftDeleteAsync(Guid id);  // New method for soft deletion
    }
}
