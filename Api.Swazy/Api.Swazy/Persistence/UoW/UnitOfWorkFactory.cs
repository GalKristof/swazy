namespace Api.Swazy.Persistence.UoW;

public class UnitOfWorkFactory(SwazyDbContext dbContext) : IUnitOfWorkFactory
{
    public IUnitOfWork Create()
    {
        return new UnitOfWork(dbContext);
    }
}
