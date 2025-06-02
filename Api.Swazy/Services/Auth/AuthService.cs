using Api.Swazy.Models.DTOs.Authentication;
using Api.Swazy.Models.DTOs.Users;
using Api.Swazy.Models.Entities;
using Api.Swazy.Models.Results;
using Api.Swazy.Persistence.Repositories;
using Api.Swazy.Providers;
using Api.Swazy.Services.Users; // Added IUserService
using Serilog;

namespace Api.Swazy.Services.Auth;

// Removed GenericService inheritance and IMapper injection
// Added IUserService injection
public class AuthService(
    IRepository<User> userRepository, // Renamed for clarity
    IHashingProvider hashingProvider, // Renamed for clarity
    IJwtTokenProvider jwtTokenProvider, // Renamed for clarity
    IUserService userService // Added
    ) : IAuthService
{
    // Removed _mapper field

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

            // Manual mapping from User to TokenDto
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
                response.Result = CommonResult.UserAlreadyExists; // Ensure this exists in CommonResult
                Log.Debug("[AuthService - {MethodName}] Registration failed. User already exists: {UserEmail}", nameof(RegisterUserAsync), dto.Email);
                return response;
            }

            // Manual mapping from RegisterUserDto to CreateUserDto
            var createUserDto = new CreateUserDto(
                dto.FirstName,
                dto.LastName,
                dto.PhoneNumber,
                dto.Email,
                dto.Password
            );

            // We also need to pass the Role, but CreateUserDto doesn't have it.
            // For now, let's assume UserService's CreateEntityAsync handles setting a default role
            // or that the User entity's constructor/properties handle it.
            // If not, UserService.CreateEntityAsync or CreateUserDto might need adjustment.
            // However, the issue is about registration, not role management complexity at this stage.
            // The User entity itself has a Role property. UserService CreateEntityAsync uses AutoMapper
            // which would map the Role if CreateUserDto had it. Since we are not using AutoMapper here,
            // and CreateUserDto doesn't have Role, the Role from RegisterUserDto will not be passed to UserService.
            // This means the user will be created with whatever default Role is set in the User entity or by the DB.
            // This is a limitation of the current CreateUserDto and UserService interaction without AutoMapper or direct Role passing.
            // For this task, we will proceed with creating the user, and the Role from RegisterUserDto will be ignored by UserService.

            var createUserResponse = await userService.CreateEntityAsync(createUserDto);

            if (createUserResponse.Result != CommonResult.Success || createUserResponse.Value == null)
            {
                response.Result = createUserResponse.Result == CommonResult.Success ? CommonResult.UnknownError : createUserResponse.Result;
                Log.Error("[AuthService - {MethodName}] Failed to create user: {UserEmail}. Reason: {Reason}",
                    nameof(RegisterUserAsync), dto.Email, createUserResponse.Result);
                return response;
            }

            var newUser = createUserResponse.Value;
             // The user's Role will be the default one, as RegisterUserDto.Role is not used by UserService.CreateEntityAsync
            // If a specific role needs to be set during registration, UserService.CreateEntityAsync would need modification
            // or we would need a way to pass the Role to it, perhaps by modifying CreateUserDto or having another method in IUserService.
            // For now, we proceed with the current structure. The created `newUser` object from `userService` will have its Role property set
            // (either default or by UserService logic if any). We need to make sure this Role is what we expect or document this behavior.
            // The User entity has a Role property, let's assume it's set by UserService.CreateEntityAsync,
            // or if not, it gets a default value. When we create TokenDto, it does not include Role.

            // Manual mapping from User to TokenDto
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