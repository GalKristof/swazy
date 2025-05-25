using Api.Swazy.Models.Base;
using Api.Swazy.Models.Results;
using Api.Swazy.Persistence.UoW;

namespace Api.Swazy.Services.Generic;

public interface IUpdateEntity<TGetDto, in TUpdateDto> where TGetDto : BaseEntity
{
    Task<CommonResponse<TGetDto?>> UpdateEntityAsync(TUpdateDto dto, IUnitOfWork? unitOfWork = null);
}
