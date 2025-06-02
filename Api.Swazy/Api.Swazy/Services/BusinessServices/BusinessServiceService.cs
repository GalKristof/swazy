using Api.Swazy.Models.DTOs.BusinessServices;
using Api.Swazy.Models.Entities;
using Api.Swazy.Persistence.Repositories;
using Api.Swazy.Services.Generic;
using AutoMapper;

namespace Api.Swazy.Services.BusinessServices
{
    public class BusinessServiceService(
        IRepository<BusinessService> defaultRepository, 
        IMapper mapper) :
        GenericService<BusinessService, CreateBusinessServiceDto, UpdateBusinessServiceDto>(defaultRepository, mapper), IBusinessServiceService;
}