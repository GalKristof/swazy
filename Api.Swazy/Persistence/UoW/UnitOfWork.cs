using Api.Swazy.Models.Base;
using Api.Swazy.Persistence.Repositories;
using Microsoft.EntityFrameworkCore.Storage;

namespace Api.Swazy.Persistence.UoW;

public class UnitOfWork(SwazyDbContext context) : IUnitOfWork
{
    private IDbContextTransaction? transaction;

    public IRepository<T> Repository<T>() where T : BaseEntity
    {
        return new Repository<T>(context);
    }

    public async Task BeginTransactionAsync()
    {
        transaction ??= await context.Database.BeginTransactionAsync();
    }

    public async Task CommitAsync()
    {
        try
        {
            await context.SaveChangesAsync();
            if (transaction != null)
            {
                await transaction.CommitAsync();
                await transaction.DisposeAsync();
                transaction = null;
            }
        }
        catch
        {
            await RollbackAsync();
            throw;
        }
    }

    public async Task RollbackAsync()
    {
        if (transaction != null)
        {
            await transaction.RollbackAsync();
            await transaction.DisposeAsync();
            transaction = null;
        }
    }

    public void Dispose()
    {
        context.Dispose();
        transaction?.Dispose();
    }
}
