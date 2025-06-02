using Api.Swazy.Models.DTOs.BusinessServices;
using Api.Swazy.Models.Entities;
using Api.Swazy.Models.Results;
using Api.Swazy.Persistence.Repositories;
using Api.Swazy.Services.Generic;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace Api.Swazy.Services.BusinessServices
{
    public class BusinessServiceService(
        IRepository<BusinessService> defaultRepository,
        IMapper mapper) :
        GenericService<BusinessService, CreateBusinessServiceDto, UpdateBusinessServiceDto>(defaultRepository, mapper), IBusinessServiceService
    {
        public async Task<OperationResult<IEnumerable<BusinessServiceDto>>> GetBusinessServicesByBusinessIdAsync(Guid businessId)
        {
            try
            {
                var services = await _defaultRepository.FindByCondition(s => s.BusinessId == businessId)
                                                       .ProjectTo<BusinessServiceDto>(_mapper.ConfigurationProvider)
                                                       .ToListAsync();

                if (services == null || !services.Any())
                {
                    return OperationResult<IEnumerable<BusinessServiceDto>>.Failure(
                        new Error((int)HttpStatusCode.NotFound, $"No services found for business ID {businessId}."));
                }

                return OperationResult<IEnumerable<BusinessServiceDto>>.Success(services);
            }
            catch (Exception ex)
            {
                // Log the exception ex here
                return OperationResult<IEnumerable<BusinessServiceDto>>.Failure(
                    new Error((int)HttpStatusCode.InternalServerError, $"An error occurred while fetching services for business ID {businessId}: {ex.Message}"));
            }
        }
    }
}