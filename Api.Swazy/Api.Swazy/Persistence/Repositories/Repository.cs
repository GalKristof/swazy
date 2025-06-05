using Api.Swazy.Models.Base;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Api.Swazy.Persistence.Repositories;

public class Repository<T> : IRepository<T> where T : BaseEntity
{
    private readonly SwazyDbContext context;
    private readonly DbSet<T> dbSet;

    public Repository(SwazyDbContext context)
    {
        this.context = context;
        dbSet = this.context.Set<T>();
    }

    public async Task<IEnumerable<T>> GetAllAsync()
    {
        return await dbSet.ToListAsync();
    }

    public async Task<T?> GetByIdAsync(Guid id)
    {
        return await dbSet.FindAsync(id);
    }

    public async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate)
    {
        return await dbSet.Where(predicate).ToListAsync();
    }

    public async Task<T?> SingleOrDefaultAsync(Expression<Func<T, bool>> predicate)
    {
        return await dbSet.SingleOrDefaultAsync(predicate);
    }

    public async Task<T> AddAsync(T entity)
    {
        await dbSet.AddAsync(entity);
        return entity;
    }

    public async Task<T> UpdateAsync(T entity)
    {
        dbSet.Update(entity);
        return entity;
    }

    public async Task DeleteAsync(Guid id)
    {
        var entity = await GetByIdAsync(id);
        if (entity != null)
        {
            dbSet.Remove(entity);
        }
    }

    public async Task<T> SoftDeleteAsync(T entity)
    {
        entity.IsDeleted = true;
        entity.DeletedAt = DateTimeOffset.UtcNow;
        context.Update(entity);
        return entity;
    }

    public async Task<T?> RestoreAsync(Guid id)
    {
        var entity = await GetSoftDeletedEntityByIdAsync(id);
        if (entity != null)
        {
            entity.IsDeleted = false;
            entity.DeletedAt = null;
            context.Update(entity);
        }
        return entity;
    }

    public Task<T?> GetSoftDeletedEntityByIdAsync(Guid id)
    {
        return context.Set<T>()
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(e => e.Id == id);
    }

    public async Task SaveChangesAsync()
    {
        await context.SaveChangesAsync();
    }

    public IQueryable<T> GetQueryable() // Implementation of new method
    {
        return dbSet;
    }
}