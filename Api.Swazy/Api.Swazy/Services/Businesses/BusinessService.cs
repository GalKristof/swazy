using Api.Swazy.Models.DTOs.Businesses;
using Api.Swazy.Models.Entities;
using Api.Swazy.Models.Results;
using Api.Swazy.Persistence.Repositories;
using Api.Swazy.Persistence.UoW;
using Api.Swazy.Services.Generic;
using Api.Swazy.Services.Users;
using Api.Swazy.Types;
using AutoMapper;
using Serilog;

namespace Api.Swazy.Services.Businesses;

public class BusinessService(
    IRepository<Business> businessRepository,
    IUserService userService,
    IMapper mapper)
    : GenericService<Business, CreateBusinessDto, UpdateBusinessDto>(businessRepository, mapper), IBusinessService
{
    public async Task<CommonResponse<Business?>> AddEmployeeAsync(AddEmployeeToBusinessDto dto, IUnitOfWork? unitOfWork = null)
    {
        Log.Verbose("[BusinessService - {MethodName}] Invoked. ", 
            nameof(AddEmployeeAsync));
        
        var response = new CommonResponse<Business?>();

        try
        {
            var repository = unitOfWork?.Repository<Business>() ?? businessRepository;
            var business = await repository.GetByIdAsync(dto.BusinessId);

            if (business == null)
            {
                response.Value = null;
                response.Result = CommonResult.NotFound;
                Log.Debug("[BusinessService - {MethodName}] Returned NotFound (Business). {BusinessId}",
                    nameof(AddEmployeeAsync), dto.BusinessId);
                return response;
            }
                
            var userResult = await userService.GetEntityByPropertyAsync(x => x.Email == dto.UserEmail);
            
            if (userResult.Result != CommonResult.Success || userResult.Value == null)
            { 
                response.Value = null;
                response.Result = CommonResult.RequirementNotFound;
                Log.Debug("[BusinessService - {MethodName}] Returned RequirementNotFound (User). {UserEmail}",
                    nameof(AddEmployeeAsync), dto.UserEmail);
                return response;
            }

            business.Employees[userResult.Value.Id] = dto.Role;
            
            await repository.UpdateAsync(business);

            if (unitOfWork is null)
            {
                await businessRepository.SaveChangesAsync();
            }
            
            response.Value = business;
            response.Result = CommonResult.Success;
            Log.Debug("[BusinessService - {MethodName}] Successfully added employee and returned. {BusinessId}",
                nameof(AddEmployeeAsync), dto.BusinessId);
        }
        catch (Exception ex)
        {
            unitOfWork?.RollbackAsync();
            response.Value = null;
            response.Result = CommonResult.UnknownError;
            Log.Error("[BusinessService- {MethodName}] An error occured. {BusinessId}"
                      + "Exception thrown: {Exception}", nameof(AddEmployeeAsync), dto.BusinessId, ex);
        }
        return response;
    }
}