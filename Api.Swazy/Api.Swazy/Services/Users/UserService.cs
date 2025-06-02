using Api.Swazy.Models.DTOs.Users;
using Api.Swazy.Models.Entities;
using Api.Swazy.Models.Results;
using Api.Swazy.Persistence.Repositories;
using Api.Swazy.Persistence.UoW;
using Api.Swazy.Providers;
using Api.Swazy.Services.Generic;
using AutoMapper;
using Serilog;

namespace Api.Swazy.Services.Users;

public class UserService(
    IRepository<User> defaultRepository,
    IHashingProvider provider,
    IMapper mapper)
    : GenericService<User, CreateUserDto, UpdateUserDto>(defaultRepository, mapper), IUserService
{
    public override async Task<CommonResponse<User?>> CreateEntityAsync(CreateUserDto dto, IUnitOfWork? unitOfWork = null)
    {
        Log.Verbose("[UserService - {MethodName}] Invoked. {UserEmail}", 
            nameof(CreateEntityAsync), dto.Email);
        
        var response = new CommonResponse<User?>();
        try
        {
            var repository = unitOfWork?.Repository<User>() ?? defaultRepository;
            
            var newUser = mapper.Map<User>(dto);
            Log.Verbose("[UserService - {MethodName}] Hashing password for new user. {UserEmail}",
                nameof(CreateEntityAsync), dto.Email);
            newUser.HashedPassword = provider.HashPassword(dto.Password);

            Log.Debug("[UserService - {MethodName}] Creating new User. {UserEmail}",
                nameof(CreateEntityAsync), dto.Email);
            
            var createdUser = await repository.AddAsync(newUser);

            if (unitOfWork is null)
            {
                await defaultRepository.SaveChangesAsync();
            }

            response.Value = createdUser;
            response.Result = CommonResult.Success;
            Log.Debug("[UserService - {MethodName}] Successfully created and returned. {UserEmail} {EntityId}",
                nameof(CreateEntityAsync), dto.Email, createdUser.Id);
            return response;
        }
        catch (Exception ex)
        {
            response.Value = null;
            response.Result = CommonResult.UnknownError;
            Log.Error("[UserService - {MethodName})] An error occured. {UserEmail} "
                      + "Exception thrown: {Exception}", nameof(CreateEntityAsync), dto.Email, ex);
            return response;
        }
    }
}

