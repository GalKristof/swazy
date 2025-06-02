using Api.Swazy.Models.DTOs.BusinessServices;
using Api.Swazy.Models.Entities;
using Api.Swazy.Models.Results;
using Api.Swazy.Persistence.Repositories;
using Api.Swazy.Services.Generic;
using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Api.Swazy.Services.BusinessServices
{
    public class BusinessServiceService(
        IRepository<BusinessService> defaultRepository,
        IMapper mapper) :
        GenericService<BusinessService, CreateBusinessServiceDto, UpdateBusinessServiceDto>(defaultRepository, mapper), IBusinessServiceService
    {
        public async Task<CommonResponse<IEnumerable<BusinessService?>>> GetBusinessServicesByBusinessIdAsync(Guid businessId)
        {
            var response = new CommonResponse<IEnumerable<BusinessService?>>();
            try
            {
                var businessServices = await defaultRepository.FindAsync(bs => bs.BusinessId == businessId);

                if (!businessServices.Any())
                {
                    response.Result = CommonResult.NotFound;
                    response.Value = [];
                }
                else
                {
                    response.Value = mapper.Map<IEnumerable<BusinessService?>>(businessServices);
                    response.Result = CommonResult.Success;
                }
            }
            catch (Exception ex)
            {
                response.Result = CommonResult.UnknownError;
                response.Value = null;
            }
            return response;
        }
    }
}