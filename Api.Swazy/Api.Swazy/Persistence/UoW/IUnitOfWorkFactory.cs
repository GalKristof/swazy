namespace Api.Swazy.Persistence.UoW;

public interface IUnitOfWorkFactory
{
    IUnitOfWork Create();
}
