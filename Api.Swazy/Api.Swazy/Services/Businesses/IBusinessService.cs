using Api.Swazy.Models.DTOs.BusinessEmployees;
using Api.Swazy.Models.DTOs.Businesses;
using Api.Swazy.Models.Entities;
using Api.Swazy.Models.Results;
using Api.Swazy.Persistence.UoW; // Keep one
using Api.Swazy.Services.Generic; // Keep one
using System;
using System.Collections.Generic;
// Removed duplicate Api.Swazy.Persistence.UoW and Api.Swazy.Services.Generic

namespace Api.Swazy.Services.Businesses;

public interface IBusinessService : IGenericService<Business, CreateBusinessDto, UpdateBusinessDto>
{
    Task<CommonResponse<BusinessEmployeeDto?>> AddEmployeeAsync(CreateBusinessEmployeeDto dto, Guid performingUserId, IUnitOfWork? unitOfWork = null);
    Task<CommonResponse<IEnumerable<BusinessEmployeeDto>>> GetBusinessEmployeesAsync(Guid businessId);
    Task<CommonResponse<bool>> RemoveEmployeeAsync(Guid businessId, Guid userId, Guid performingUserId);
    Task<CommonResponse<BusinessEmployeeDto?>> UpdateEmployeeRoleAsync(Guid businessId, Guid userId, UpdateBusinessEmployeeDto dto, Guid performingUserId);
}