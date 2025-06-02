using Api.Swazy.Models.DTOs.BusinessServices;
using Api.Swazy.Models.Entities;
using Api.Swazy.Models.Results;
using Api.Swazy.Services.Generic;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Api.Swazy.Services.BusinessServices
{
    public interface IBusinessServiceService : IGenericService<BusinessService, CreateBusinessServiceDto, UpdateBusinessServiceDto>
    {
        // Add any additional BusinessService specific methods here
        Task<OperationResult<IEnumerable<BusinessServiceDto>>> GetBusinessServicesByBusinessIdAsync(Guid businessId);
    }
}
