using Api.Swazy.Models.DTOs.BusinessServices;
using Api.Swazy.Models.Entities;
using Api.Swazy.Models.Results;
using Api.Swazy.Services.Generic;

namespace Api.Swazy.Services.BusinessServices
{
    public interface IBusinessServiceService : IGenericService<BusinessService, CreateBusinessServiceDto, UpdateBusinessServiceDto>
    {
        Task<CommonResponse<IEnumerable<BusinessService?>>> GetBusinessServicesByBusinessIdAsync(Guid businessId);
    }
}
