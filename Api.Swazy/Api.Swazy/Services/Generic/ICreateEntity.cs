using Api.Swazy.Models.Base;
using Api.Swazy.Models.Results;
using Api.Swazy.Persistence.UoW;

namespace Api.Swazy.Services.Generic;

public interface ICreateEntity<TEntity, in TCreateDto> where TEntity : BaseEntity
{
    Task<CommonResponse<TEntity?>> CreateEntityAsync(TCreateDto dto, IUnitOfWork? unitOfWork = null);
}
