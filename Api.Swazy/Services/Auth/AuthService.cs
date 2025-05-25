using Api.Swazy.Models.DTOs.Authentication;
using Api.Swazy.Models.DTOs.Users;
using Api.Swazy.Models.Entities;
using Api.Swazy.Models.Results;
using Api.Swazy.Persistence.Repositories;
using Api.Swazy.Providers;
using Api.Swazy.Services.Generic;
using AutoMapper;
using Serilog;

namespace Api.Swazy.Services.Auth;

public class AuthService(IRepository<User> defaultRepository,
    IHashingProvider provider,
    IJwtTokenProvider tokenProvider,
    IMapper mapper) : GenericService<User, CreateUserDto, UpdateUserDto>(defaultRepository, mapper), IAuthService
{
    public async Task<CommonResponse<string>> LoginUserAsync(LoginUserDto dto)
    {
        Log.Verbose("[UserService - {MethodName}] Invoked. {UserEmail}", 
            nameof(LoginUserAsync), dto.Email);
        
        var response = new CommonResponse<string>();
        
        try
        {
            var user = await defaultRepository.SingleOrDefaultAsync(x => x.Email == dto.Email);

            if (user == null)
            {
                response.Value = null;
                response.Result = CommonResult.InvalidCredentials;
                Log.Debug("[UserService - {MethodName}] Returned InvalidCredentials (User was null). {UserEmail}",
                    nameof(LoginUserAsync), dto.Email);
                return response;
            }

            Log.Debug("[UserService - {MethodName}] User was found, validating password. {UserEmail} {UserId}",
                nameof(LoginUserAsync), dto.Email, user.Id);
            var validate = provider.ValidatePassword(dto.Password, user.HashedPassword);

            if (!validate)
            {
                response.Value = null;
                response.Result = CommonResult.InvalidCredentials;
                Log.Debug("[UserService - {MethodName}] Returned InvalidCredentials (Incorrect Password). {UserEmail} {UserId}",
                    nameof(LoginUserAsync), dto.Email, user.Id);
                return response;
            }

            var tokenDto = mapper.Map<TokenDto>(user);
            var token = tokenProvider.GenerateAccessToken(tokenDto);

            response.Value = token;
            response.Result = CommonResult.Success;
            Log.Debug("[UserService - {MethodName}] Successfully created and returned token. {UserEmail} {UserId} {JwtToken}",
                nameof(LoginUserAsync), dto.Email, user.Id, token);
            return response;
        }
        catch (Exception ex)
        {
            response.Value = null;
            response.Result = CommonResult.UnknownError;
            Log.Error("[UserService - {MethodName})] An error occured. {UserEmail} "
                      + "Exception thrown: {Exception}", nameof(LoginUserAsync), dto.Email, ex);
            return response;
        }
    }
}