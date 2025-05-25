using Api.Swazy.Models.Base;
using Api.Swazy.Persistence.Repositories;

namespace Api.Swazy.Persistence.UoW;

public interface IUnitOfWork
{
    IRepository<T> Repository<T>() where T : BaseEntity;
    Task BeginTransactionAsync();
    Task CommitAsync();
    Task RollbackAsync();
}
