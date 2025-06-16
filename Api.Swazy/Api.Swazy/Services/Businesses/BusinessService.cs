using Api.Swazy.Models.DTOs.BusinessEmployees;
using Api.Swazy.Models.DTOs.Businesses;
using Api.Swazy.Models.Entities;
using Api.Swazy.Models.Results;
using Api.Swazy.Persistence.Repositories; // Keep one
using Api.Swazy.Persistence.UoW; // Keep one
using Api.Swazy.Services.Generic; // Keep one
using Api.Swazy.Services.Users; // Keep one
using Api.Swazy.Types; // Keep one
using AutoMapper; // Keep one
using Microsoft.EntityFrameworkCore;
using Serilog; // Keep one
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
// Removed duplicate using groups that were identical
using Api.Swazy.Persistence.Repositories;
using Api.Swazy.Persistence.UoW;
using Api.Swazy.Services.Generic;
using Api.Swazy.Services.Users;
using Api.Swazy.Types;
using AutoMapper;
using Serilog;

namespace Api.Swazy.Services.Businesses;

public class BusinessService(
    IRepository<Business> businessRepository, // This is the base._repository for GenericService
    IRepository<BusinessEmployee> businessEmployeeRepository,
    IUserService userService,
    IMapper mapper) // This is the base._mapper for GenericService
    : GenericService<Business, CreateBusinessDto, UpdateBusinessDto>(businessRepository, mapper), IBusinessService
{
    private readonly IRepository<BusinessEmployee> _businessEmployeeRepository = businessEmployeeRepository;
    private readonly IUserService _userService = userService; // Store userService for clarity if needed, or use directly
    // No need to re-declare _repository or _mapper if base class makes them accessible (e.g. protected)
    // However, the errors CS0103 suggest they are not directly accessible with _ prefix from derived class,
    // so we should use the constructor parameters directly or assign them to private fields if base does not expose.
    // For this fix, we'll assume base class (GenericService) uses its constructor params internally
    // and we use our own specific ones here.

    public async Task<CommonResponse<BusinessEmployeeDto?>> AddEmployeeAsync(CreateBusinessEmployeeDto dto, Guid performingUserId, IUnitOfWork? unitOfWork = null)
    {
        Log.Verbose("[BusinessService - {MethodName}] Invoked for BusinessId: {BusinessId}, UserEmail: {UserEmail}, PerformingUserId: {PerformingUserId}",
            nameof(AddEmployeeAsync), dto.BusinessId, dto.UserEmail, performingUserId);

        var response = new CommonResponse<BusinessEmployeeDto?>();

        try
        {
            var businessRepo = unitOfWork?.Repository<Business>() ?? businessRepository; // Use injected businessRepository
            var business = await businessRepo.GetByIdAsync(dto.BusinessId);

            if (business == null)
            {
                response.Result = CommonResult.NotFound;
                // response.Message removed
                Log.Debug("[BusinessService - {MethodName}] Business not found. {BusinessId}", nameof(AddEmployeeAsync), dto.BusinessId);
                return response;
            }

            var userResult = await _userService.GetEntityByPropertyAsync(x => x.Email == dto.UserEmail);
            if (userResult.Result != CommonResult.Success || userResult.Value == null)
            {
                response.Result = CommonResult.RequirementNotFound;
                // response.Message removed
                Log.Debug("[BusinessService - {MethodName}] User not found by email. {UserEmail}", nameof(AddEmployeeAsync), dto.UserEmail);
                return response;
            }
            var userId = userResult.Value.Id;

            var beRepository = unitOfWork?.Repository<BusinessEmployee>() ?? _businessEmployeeRepository;
            // Use GetQueryable().FirstOrDefaultAsync()
            var existingEmployee = await beRepository.GetQueryable().FirstOrDefaultAsync(be => be.BusinessId == dto.BusinessId && be.UserId == userId && !be.IsDeleted);
            if (existingEmployee != null) // Already checked !be.IsDeleted in query
            {
                response.Result = CommonResult.AlreadyIncluded;
                // response.Message removed
                Log.Debug("[BusinessService - {MethodName}] User is already an employee. BusinessId: {BusinessId}, UserId: {UserId}", nameof(AddEmployeeAsync), dto.BusinessId, userId);
                return response;
            }

            var hiringUserResult = await _userService.GetEntityByPropertyAsync(u => u.Id == performingUserId);
            if (hiringUserResult.Result != CommonResult.Success || hiringUserResult.Value == null)
            {
                response.Result = CommonResult.RequirementNotFound;
                // response.Message removed
                Log.Debug("[BusinessService - {MethodName}] Hiring user not found. {PerformingUserId}", nameof(AddEmployeeAsync), performingUserId);
                return response;
            }

            var newEmployee = new BusinessEmployee
            {
                BusinessId = dto.BusinessId,
                UserId = userId,
                Role = dto.Role,
                HiredDate = DateTimeOffset.UtcNow,
                HiredBy = performingUserId
            };

            await beRepository.AddAsync(newEmployee);

            if (unitOfWork == null)
            {
                await beRepository.SaveChangesAsync();
            }

            var createdEmployeeWithIncludes = await beRepository.GetQueryable()
                .Include(i => i.User)
                .Include(i => i.HiredByUser)
                .FirstOrDefaultAsync(be => be.Id == newEmployee.Id);

            response.Value = mapper.Map<BusinessEmployeeDto>(createdEmployeeWithIncludes); // Use injected mapper
            response.Result = CommonResult.Success;
            Log.Debug("[BusinessService - {MethodName}] Successfully added employee. BusinessId: {BusinessId}, UserId: {UserId}", nameof(AddEmployeeAsync), dto.BusinessId, userId);
        }
        catch (Exception ex)
        {
            await (unitOfWork?.RollbackAsync() ?? Task.CompletedTask);
            response.Result = CommonResult.UnknownError;
            // response.Message removed
            Log.Error(ex, "[BusinessService - {MethodName}] An error occurred. BusinessId: {BusinessId}, UserEmail: {UserEmail}. Exception: {ExceptionMessage}",
                nameof(AddEmployeeAsync), dto.BusinessId, dto.UserEmail, ex.Message);
        }
        return response;
    }

    public async Task<CommonResponse<IEnumerable<BusinessEmployeeDto>>> GetBusinessEmployeesAsync(Guid businessId)
    {
        Log.Verbose("[BusinessService - {MethodName}] Invoked for BusinessId: {BusinessId}", nameof(GetBusinessEmployeesAsync), businessId);
        var response = new CommonResponse<IEnumerable<BusinessEmployeeDto>>();

        try
        {
            var business = await businessRepository.GetByIdAsync(businessId); // Use injected businessRepository (base._repository)
            if (business == null)
            {
                response.Result = CommonResult.NotFound;
                // response.Message removed
                Log.Debug("[BusinessService - {MethodName}] Business not found. {BusinessId}", nameof(GetBusinessEmployeesAsync), businessId);
                return response;
            }

            var employees = await _businessEmployeeRepository.GetQueryable()
                .Where(be => be.BusinessId == businessId)
                .Include(i => i.User)
                .Include(i => i.HiredByUser)
                .ToListAsync();

            response.Value = mapper.Map<IEnumerable<BusinessEmployeeDto>>(employees); // Use injected mapper
            response.Result = CommonResult.Success;
            Log.Debug("[BusinessService - {MethodName}] Successfully retrieved employees for BusinessId: {BusinessId}, Count: {Count}", nameof(GetBusinessEmployeesAsync), businessId, employees.Count());
        }
        catch (Exception ex)
        {
            response.Result = CommonResult.UnknownError;
            // response.Message removed
            Log.Error(ex, "[BusinessService - {MethodName}] An error occurred for BusinessId: {BusinessId}. Exception: {ExceptionMessage}",
                nameof(GetBusinessEmployeesAsync), businessId, ex.Message);
        }
        return response;
    }

    public async Task<CommonResponse<bool>> RemoveEmployeeAsync(Guid businessId, Guid userId, Guid performingUserId)
    {
        Log.Verbose("[BusinessService - {MethodName}] Invoked for BusinessId: {BusinessId}, UserId: {UserId}, PerformingUserId: {PerformingUserId}",
            nameof(RemoveEmployeeAsync), businessId, userId, performingUserId);
        var response = new CommonResponse<bool>();

        try
        {
            var employeeToDelete = await _businessEmployeeRepository.GetQueryable()
                .FirstOrDefaultAsync(be => be.BusinessId == businessId && be.UserId == userId && !be.IsDeleted);
            
            if (employeeToDelete == null)
            {
                response.Result = CommonResult.NotFound;
                // response.Message removed
                Log.Debug("[BusinessService - {MethodName}] Employee not found or already (soft)deleted. BusinessId: {BusinessId}, UserId: {UserId}", nameof(RemoveEmployeeAsync), businessId, userId);
                return response;
            }

            // Using SoftDeleteAsync from IRepository if it handles setting IsDeleted and saving.
            // The IRepository has SoftDeleteAsync(T entity) which marks IsDeleted = true and DeletedAt.
            // It also has DeleteAsync(Guid id) which does a hard delete.
            // The task asks for soft-delete.
            await _businessEmployeeRepository.SoftDeleteAsync(employeeToDelete);
            await _businessEmployeeRepository.SaveChangesAsync(); // SoftDeleteAsync might not save changes itself.

            response.Value = true;
            response.Result = CommonResult.Success;
            Log.Debug("[BusinessService - {MethodName}] Successfully removed employee. BusinessId: {BusinessId}, UserId: {UserId}", nameof(RemoveEmployeeAsync), businessId, userId);
        }
        catch (Exception ex)
        {
            response.Result = CommonResult.UnknownError;
            // response.Message removed
            Log.Error(ex, "[BusinessService - {MethodName}] An error occurred. BusinessId: {BusinessId}, UserId: {UserId}. Exception: {ExceptionMessage}",
                nameof(RemoveEmployeeAsync), businessId, userId, ex.Message);
        }
        return response;
    }

    public async Task<CommonResponse<BusinessEmployeeDto?>> UpdateEmployeeRoleAsync(Guid businessId, Guid userId, UpdateBusinessEmployeeDto dto, Guid performingUserId)
    {
        Log.Verbose("[BusinessService - {MethodName}] Invoked for BusinessId: {BusinessId}, UserId: {UserId}, PerformingUserId: {PerformingUserId}",
            nameof(UpdateEmployeeRoleAsync), businessId, userId, performingUserId);
        var response = new CommonResponse<BusinessEmployeeDto?>();

        try
        {
            var employeeToUpdate = await _businessEmployeeRepository.GetQueryable()
                .Include(i => i.User)
                .Include(i => i.HiredByUser)
                .FirstOrDefaultAsync(be => be.BusinessId == businessId && be.UserId == userId && !be.IsDeleted);

            if (employeeToUpdate == null)
            {
                response.Result = CommonResult.NotFound;
                // response.Message removed
                Log.Debug("[BusinessService - {MethodName}] Employee not found or (soft)deleted. BusinessId: {BusinessId}, UserId: {UserId}", nameof(UpdateEmployeeRoleAsync), businessId, userId);
                return response;
            }

            employeeToUpdate.Role = dto.Role;

            await _businessEmployeeRepository.UpdateAsync(employeeToUpdate);
            await _businessEmployeeRepository.SaveChangesAsync();
            
            response.Value = mapper.Map<BusinessEmployeeDto>(employeeToUpdate); // Use injected mapper
            response.Result = CommonResult.Success;
            Log.Debug("[BusinessService - {MethodName}] Successfully updated employee role. BusinessId: {BusinessId}, UserId: {UserId}", nameof(UpdateEmployeeRoleAsync), businessId, userId);
        }
        catch (Exception ex)
        {
            response.Result = CommonResult.UnknownError;
            // response.Message removed
            Log.Error(ex, "[BusinessService - {MethodName}] An error occurred. BusinessId: {BusinessId}, UserId: {UserId}. Exception: {ExceptionMessage}",
                nameof(UpdateEmployeeRoleAsync), businessId, userId, ex.Message);
        }
        return response;
    }
}