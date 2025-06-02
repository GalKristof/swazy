using Api.Swazy.Models.Base;
using Api.Swazy.Models.DTOs;
using Api.Swazy.Models.Results;
using Api.Swazy.Persistence.Repositories;
using Api.Swazy.Persistence.UoW;
using AutoMapper;
using Serilog;
using System.Linq.Expressions;

namespace Api.Swazy.Services.Generic;

public class GenericService<TEntity, TCreateDto, TUpdateDto>(
    IRepository<TEntity> defaultRepository,
    IMapper mapper) 
    : IGenericService<TEntity, TCreateDto, TUpdateDto>
    where TEntity : BaseEntity
    where TUpdateDto : BaseUpdateDto
{

    public virtual async Task<CommonResponse<TEntity?>> CreateEntityAsync(TCreateDto dto, IUnitOfWork? unitOfWork = null)
    {
        var entityName = typeof(TEntity).Name;
        Log.Verbose("[GenericService ({EntityName}) - {MethodName}] Invoked. ", 
            entityName, nameof(CreateEntityAsync));
        
        var response = new CommonResponse<TEntity?>();

        try
        {
            var repository = unitOfWork?.Repository<TEntity>() ?? defaultRepository;
            var entity = mapper.Map<TEntity>(dto);
            var createdEntity = await repository.AddAsync(entity);

            if (unitOfWork is null)
            {
                await defaultRepository.SaveChangesAsync();
            }

            response.Value = createdEntity;
            response.Result = CommonResult.Success;
            Log.Debug("[GenericService ({EntityName}) - {MethodName}] Successfully created and returned. {EntityId}",
                entityName, nameof(CreateEntityAsync), createdEntity.Id);
        }
        catch (Exception ex)
        {
            response.Value = null;
            response.Result = CommonResult.UnknownError;
            Log.Error("[GenericService ({EntityName}) - {MethodName}] An error occured. "
                      + "Exception thrown: {Exception}", entityName, nameof(CreateEntityAsync), ex);
        }
        return response;
    }

    public virtual async Task<CommonResponse<IEnumerable<TEntity>>> GetAllEntitiesAsync()
    {
        var entityName = typeof(TEntity).Name;
        Log.Verbose("[GenericService ({EntityName}) - {MethodName}] Invoked. ", 
            entityName, nameof(GetAllEntitiesAsync));
        
        var response = new CommonResponse<IEnumerable<TEntity>>();

        try
        {
            var entities = await defaultRepository.GetAllAsync();
            response.Value = entities;
            response.Result = CommonResult.Success;
            Log.Debug("[GenericService ({EntityName}) - {MethodName}] Successfully returned "
                      + "{NumberOfEntities} number of entities.",
                entityName, nameof(GetAllEntitiesAsync), entities.Count());
        }
        catch (Exception ex)
        {
            response.Value = [];
            response.Result = CommonResult.UnknownError;
            Log.Error("[GenericService ({EntityName}) - {MethodName}] An error occured. "
                      + "Exception thrown: {Exception}", 
                entityName, nameof(GetAllEntitiesAsync), ex);
        }

        return response;
    }

    public virtual async Task<CommonResponse<TEntity?>> DeleteEntityAsync(Guid id, IUnitOfWork? unitOfWork = null)
    {
        var entityName = typeof(TEntity).Name;
        Log.Verbose("[GenericService ({EntityName}) - {MethodName}] Invoked. {EntityId}", 
            entityName, nameof(DeleteEntityAsync), id);
        
        var response = new CommonResponse<TEntity?>();

        try
        {
            var repository = unitOfWork?.Repository<TEntity>() ?? defaultRepository;
            var entity = await repository.GetByIdAsync(id);

            if (entity == null)
            {
                response.Value = null;
                response.Result = CommonResult.NotFound;
                Log.Debug("[GenericService ({EntityName}) - {MethodName}] Returned NotFound. {EntityId}",
                    entityName, nameof(DeleteEntityAsync), id);
                return response;
            }

            var deleteEntity = await repository.SoftDeleteAsync(entity);

            if (unitOfWork is null)
            {
                await defaultRepository.SaveChangesAsync();
            }
            
            response.Value = deleteEntity;
            response.Result = CommonResult.Success;
            Log.Debug("[GenericService ({EntityName}) - {MethodName}] Successfully made a soft delete. {EntityId}",
                entityName, nameof(DeleteEntityAsync), id);
        }
        catch (Exception ex)
        {
            response.Value = null;
            response.Result = CommonResult.UnknownError;
            Log.Error("[GenericService ({EntityName}) - {MethodName}] An error occured. {EntityId}"
                      + "Exception thrown: {Exception}", 
                entityName, nameof(DeleteEntityAsync), id, ex);
        }
        return response;
    }

    public virtual async Task<CommonResponse<TEntity?>> UpdateEntityAsync(TUpdateDto dto, IUnitOfWork? unitOfWork = null)
    {
        var entityName = typeof(TEntity).Name;
        Log.Verbose("[GenericService ({EntityName}) - {MethodName}] Invoked. {EntityId}", 
            entityName, nameof(UpdateEntityAsync), dto.Id);
        var response = new CommonResponse<TEntity?>();
        
        try
        {
            var repository = unitOfWork?.Repository<TEntity>() ?? defaultRepository;
            var entity = await repository.GetByIdAsync(dto.Id);

            if (entity == null)
            {
                response.Value = null;
                response.Result = CommonResult.NotFound;
                Log.Debug("[GenericService ({EntityName}) - {MethodName}] Returned NotFound. {EntityId}",
                    entityName, nameof(UpdateEntityAsync), dto.Id);
                return response;
            }

            mapper.Map(dto, entity);
            
            await repository.UpdateAsync(entity);

            if (unitOfWork is null)
            {
                await defaultRepository.SaveChangesAsync();
            }
            
            response.Value = entity;
            response.Result = CommonResult.Success;
            
            Log.Debug("[GenericService ({EntityName}) - {MethodName}] Successfully made an update. {EntityId}",
                entityName, nameof(UpdateEntityAsync), dto.Id);
        }
        catch (Exception ex)
        {
            response.Value = null;
            response.Result = CommonResult.UnknownError;
            Log.Error("[GenericService ({EntityName}) - {MethodName}] An error occured. {EntityId}"
                      + "Exception thrown: {Exception}", 
                entityName, nameof(UpdateEntityAsync), dto.Id, ex);
        }
        return response;
    }

    public async Task<CommonResponse<TEntity?>> GetSingleEntityByIdAsync(Guid id)
    {
        var response = new CommonResponse<TEntity?>();
        var entityName = typeof(TEntity).Name;
        Log.Verbose("[GenericService ({EntityName}) - {MethodName}] Invoked. {EntityId}", 
            entityName, nameof(GetSingleEntityByIdAsync), id);

        try
        {
            var entity = await defaultRepository.SingleOrDefaultAsync(x => x.Id == id);

            if (entity == null)
            {
                response.Value = null;
                response.Result = CommonResult.NotFound;
                Log.Debug("[GenericService ({EntityName}) - {MethodName}] Returned NotFound. {EntityId}",
                    entityName, nameof(GetSingleEntityByIdAsync), id);
                return response;
            }

            response.Value = entity;
            response.Result = CommonResult.Success;
            Log.Debug("[GenericService ({EntityName}) - {MethodName}] Successfully returned entity. {EntityId}",
                entityName, nameof(GetSingleEntityByIdAsync), id);
        }
        catch (Exception ex)
        {
            response.Value = null;
            response.Result = CommonResult.UnknownError;
            Log.Error("[GenericService ({EntityName}) - {MethodName}] An error occured. {EntityId}"
                      + "Exception thrown: {Exception}", 
                entityName, nameof(GetSingleEntityByIdAsync), id, ex);
        }
        return response;
    }
    
    public async Task<CommonResponse<TEntity?>> GetEntityByPropertyAsync(Expression<Func<TEntity, bool>> predicate)
    {
        var response = new CommonResponse<TEntity?>();
        var entityName = typeof(TEntity).Name;
        Log.Verbose("[GenericService ({EntityName}) - {MethodName}] Invoked. {Predicate}", 
            entityName, nameof(GetEntityByPropertyAsync), predicate);

        try
        {
            var entity = await defaultRepository.SingleOrDefaultAsync(predicate);

            if (entity == null)
            {
                response.Value = null;
                response.Result = CommonResult.NotFound;
                Log.Debug("[GenericService ({EntityName}) - {MethodName}] Returned NotFound. {Predicate}",
                    entityName, nameof(GetEntityByPropertyAsync), predicate);
                return response;
            }

            response.Value = entity;
            response.Result = CommonResult.Success;
            Log.Debug("[GenericService ({EntityName}) - {MethodName}] Successfully returned entity. {Predicate} {EntityId}",
                entityName, nameof(GetEntityByPropertyAsync), predicate, entity.Id);
        }
        catch (Exception ex)
        {
            response.Value = null;
            response.Result = CommonResult.UnknownError;
            Log.Error("[GenericService ({EntityName}) - {MethodName}] An error occured. {Predicate}"
                      + "Exception thrown: {Exception}", 
                entityName, nameof(GetEntityByPropertyAsync), predicate, ex);
        }
        return response;
    }
}

