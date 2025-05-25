using Api.Swazy.Models.DTOs.Services;
using Api.Swazy.Models.Entities;
using Api.Swazy.Services.Generic;

namespace Api.Swazy.Services.Services;

public interface IServiceService : IGenericService<Service, CreateServiceDto, UpdateServiceDto>;