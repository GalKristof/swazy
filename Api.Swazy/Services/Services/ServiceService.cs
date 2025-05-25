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
    IUnitOfWorkFactory unitOfWorkFactory,
    IMapper mapper)
    : GenericService<Service, CreateServiceDto, UpdateServiceDto>(repository, mapper), IServiceService
{
    public override async Task<CommonResponse<Service?>> CreateEntityAsync(CreateServiceDto dto, IUnitOfWork? unitOfWork = null)
    {
        Log.Verbose("[ServiceService - {MethodName}] Invoked. ", 
            nameof(CreateEntityAsync));

        unitOfWork ??= unitOfWorkFactory.Create();
        
        var response = new CommonResponse<Service?>();
        try
        {
            await unitOfWork.BeginTransactionAsync();
            
            var service = mapper.Map<Service>(dto);

            var serviceRepository = unitOfWork.Repository<Service>();
            var createdService = await serviceRepository.AddAsync(service);

            await unitOfWork.CommitAsync();
            
            response.Value = createdService;
            response.Result = CommonResult.Success;
            Log.Debug("[ServiceService - {MethodName}] Successfully created and returned. {EntityId}",
                nameof(CreateEntityAsync), createdService.Id);
        }
        catch (DbUpdateException ex)
        {
            await unitOfWork.RollbackAsync();
            response.Value = null;
            response.Result = CommonResult.DatabaseError;
            Log.Error("[ServiceService - {MethodName}] Database error occurred. "
                      + "Exception thrown: {Exception}", nameof(CreateEntityAsync), ex);
        }
        catch (Exception ex)
        {
            await unitOfWork.RollbackAsync();
            response.Value = null;
            response.Result = CommonResult.UnknownError;
            Log.Error("[ServiceService- {MethodName}] An error occured. "
                      + "Exception thrown: {Exception}", nameof(CreateEntityAsync), ex);
        }
        finally
        {
            if (unitOfWork is UnitOfWork localUow)
            {
                localUow.Dispose();
            }
        }
        return response;
    }
}
