using Api.Swazy.Models.Base;
using System.Linq.Expressions;

namespace Api.Swazy.Persistence.Repositories;

public interface IRepository<T> where T : BaseEntity
{
    Task<IEnumerable<T>> GetAllAsync();
    Task<T?> GetByIdAsync(Guid id);
    Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate);
    Task<T?> SingleOrDefaultAsync(Expression<Func<T, bool>> predicate);
    Task<T> AddAsync(T entity);
    Task<T> UpdateAsync(T entity);
    Task DeleteAsync(Guid id);
    Task<T> SoftDeleteAsync(T entity);
    Task<T?> RestoreAsync(Guid id);
    Task<T?> GetSoftDeletedEntityByIdAsync(Guid id);
    Task SaveChangesAsync();
    IQueryable<T> GetQueryable(); // New method
}
