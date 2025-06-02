using Api.Swazy.Models.Base;
using Api.Swazy.Models.DTOs;
using Api.Swazy.Models.Results;
using Api.Swazy.Persistence.UoW;
using System.Linq.Expressions;

namespace Api.Swazy.Services.Generic;

public interface IGenericService<TEntity, in TCreateDto, in TUpdateDto> 
    : IUpdateEntity<TEntity, TUpdateDto>, ICreateEntity<TEntity, TCreateDto>
    where TEntity : BaseEntity
    where TUpdateDto : BaseUpdateDto
{
    Task<CommonResponse<IEnumerable<TEntity>>> GetAllEntitiesAsync();
    Task<CommonResponse<TEntity?>> DeleteEntityAsync(Guid id, IUnitOfWork? unitOfWork = null);
    Task<CommonResponse<TEntity?>> GetSingleEntityByIdAsync(Guid id);
    Task<CommonResponse<TEntity?>> GetEntityByPropertyAsync(Expression<Func<TEntity, bool>> predicate);
}