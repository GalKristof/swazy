using Api.Swazy.Models.DTOs.Businesses;
using Api.Swazy.Models.Entities;
using Api.Swazy.Models.Results;
using Api.Swazy.Persistence.UoW;
using Api.Swazy.Services.Generic;

namespace Api.Swazy.Services.Businesses;

public interface IBusinessService : IGenericService<Business, CreateBusinessDto, UpdateBusinessDto>
{
    Task<CommonResponse<Business?>> AddEmployeeAsync(AddEmployeeToBusinessDto dto, IUnitOfWork? unitOfWork = null);
}