using Api.Swazy.Data.Contexts;
using Api.Swazy.Models.DTOs.BusinessServices;
using Api.Swazy.Models.Entities;
using Api.Swazy.Services.Generic;
using AutoMapper;

namespace Api.Swazy.Services.BusinessServices
{
    public class BusinessServiceService : GenericService<BusinessService, CreateBusinessServiceDto, UpdateBusinessServiceDto>, IBusinessServiceService
    {
        public BusinessServiceService(ApplicationDbContext context, IMapper mapper) : base(context, mapper)
        {
        }

        // Implement any additional BusinessService specific methods here
    }
}
