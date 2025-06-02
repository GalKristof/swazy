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
        public async Task<CommonResponse<IEnumerable<GetBusinessServiceDto>>> GetBusinessServicesByBusinessIdAsync(Guid businessId)
        {
            var response = new CommonResponse<IEnumerable<GetBusinessServiceDto>>();
            try
            {
                var businessServices = await _defaultRepository.GetAsync(filter: bs => bs.BusinessId == businessId);

                if (businessServices == null || !businessServices.Any())
                {
                    response.Result = CommonResult.NotFound;
                    response.Value = Enumerable.Empty<GetBusinessServiceDto>();
                }
                else
                {
                    response.Value = _mapper.Map<IEnumerable<GetBusinessServiceDto>>(businessServices);
                    response.Result = CommonResult.Success;
                }
            }
            catch (Exception ex)
            {
                // Future: Consider adding logging here, e.g., using Serilog
                // For now, ex is not used to avoid warnings if no logging is in place.
                // To use ex, you might log: Console.WriteLine(ex.Message);
                response.Result = CommonResult.UnknownError;
                response.Value = null;
            }
            return response;
        }
    }
}