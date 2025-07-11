using Api.Swazy.Models.DTOs.Authentication;
using Api.Swazy.Models.DTOs.Users;
using Api.Swazy.Models.Entities;
using Api.Swazy.Models.Results;
using Api.Swazy.Persistence.Repositories;
using Api.Swazy.Providers;
using Api.Swazy.Services.Users;
using Serilog;

namespace Api.Swazy.Services.Auth;

public class AuthService(
    IRepository<User> userRepository,
    IHashingProvider hashingProvider,
    IJwtTokenProvider jwtTokenProvider,
    IUserService userService
    ) : IAuthService
{

    public async Task<CommonResponse<string>> LoginUserAsync(LoginUserDto dto)
    {
        Log.Verbose("[AuthService - {MethodName}] Invoked. {UserEmail}",
            nameof(LoginUserAsync), dto.Email);
        
        var response = new CommonResponse<string>();
        
        try
        {
            var user = await userRepository.SingleOrDefaultAsync(x => x.Email == dto.Email);

            if (user == null)
            {
                response.Value = null;
                response.Result = CommonResult.InvalidCredentials;
                Log.Debug("[AuthService - {MethodName}] Returned InvalidCredentials (User was null). {UserEmail}",
                    nameof(LoginUserAsync), dto.Email);
                return response;
            }

            Log.Debug("[AuthService - {MethodName}] User was found, validating password. {UserEmail} {UserId}",
                nameof(LoginUserAsync), dto.Email, user.Id);
            var validate = hashingProvider.ValidatePassword(dto.Password, user.HashedPassword);

            if (!validate)
            {
                response.Value = null;
                response.Result = CommonResult.InvalidCredentials;
                Log.Debug("[AuthService - {MethodName}] Returned InvalidCredentials (Incorrect Password). {UserEmail} {UserId}",
                    nameof(LoginUserAsync), dto.Email, user.Id);
                return response;
            }
            
            var tokenDto = new TokenDto
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName
            };
            var token = jwtTokenProvider.GenerateAccessToken(tokenDto);

            response.Value = token;
            response.Result = CommonResult.Success;
            Log.Debug("[AuthService - {MethodName}] Successfully created and returned token. {UserEmail} {UserId} {JwtToken}",
                nameof(LoginUserAsync), dto.Email, user.Id, token);
            return response;
        }
        catch (Exception ex)
        {
            response.Value = null;
            response.Result = CommonResult.UnknownError;
            Log.Error("[AuthService - {MethodName}] An error occured. {UserEmail} "
                      + "Exception thrown: {Exception}", nameof(LoginUserAsync), dto.Email, ex);
            return response;
        }
    }

    public async Task<CommonResponse<string>> RegisterUserAsync(RegisterUserDto dto)
    {
        Log.Verbose("[AuthService - {MethodName}] Invoked for {UserEmail}", nameof(RegisterUserAsync), dto.Email);
        var response = new CommonResponse<string>();

        try
        {
            var existingUser = await userRepository.SingleOrDefaultAsync(x => x.Email == dto.Email);
            if (existingUser != null)
            {
                response.Result = CommonResult.UserAlreadyExists;
                Log.Debug("[AuthService - {MethodName}] Registration failed. User already exists: {UserEmail}", nameof(RegisterUserAsync), dto.Email);
                return response;
            }
            
            var createUserDto = new CreateUserDto(
                dto.FirstName,
                dto.LastName,
                dto.PhoneNumber,
                dto.Email,
                dto.Password
            );

            var createUserResponse = await userService.CreateEntityAsync(createUserDto);

            if (createUserResponse.Result != CommonResult.Success || createUserResponse.Value == null)
            {
                response.Result = createUserResponse.Result == CommonResult.Success ? CommonResult.UnknownError : createUserResponse.Result;
                Log.Error("[AuthService - {MethodName}] Failed to create user: {UserEmail}. Reason: {Reason}",
                    nameof(RegisterUserAsync), dto.Email, createUserResponse.Result);
                return response;
            }

            var newUser = createUserResponse.Value;
            
            var tokenDto = new TokenDto
            {
                Id = newUser.Id,
                FirstName = newUser.FirstName,
                LastName = newUser.LastName
            };
            var token = jwtTokenProvider.GenerateAccessToken(tokenDto);

            response.Value = token;
            response.Result = CommonResult.Success;
            Log.Debug("[AuthService - {MethodName}] Successfully registered user and returned token. {UserEmail} {UserId} {JwtToken}",
                nameof(RegisterUserAsync), dto.Email, newUser.Id, token);
            return response;
        }
        catch (Exception ex)
        {
            response.Result = CommonResult.UnknownError;
            Log.Error("[AuthService - {MethodName}] An error occurred during registration for {UserEmail}. Exception: {Exception}",
                nameof(RegisterUserAsync), dto.Email, ex);
            return response;
        }
    }
}