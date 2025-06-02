using Api.Swazy.Models.DTOs.Services;
using Api.Swazy.Models.Entities;
using Api.Swazy.Models.Results;
using Api.Swazy.Persistence.Repositories;
using Api.Swazy.Persistence.UoW;
using Api.Swazy.Services.Generic;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace Api.Swazy.Services.Services;

public class ServiceService(
    IRepository<Service> repository,
    IMapper mapper)
    : GenericService<Service, CreateServiceDto, UpdateServiceDto>(repository, mapper), IServiceService;