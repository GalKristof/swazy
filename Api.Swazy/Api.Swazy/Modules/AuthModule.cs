using System.Net;
using System.IdentityModel.Tokens.Jwt;
using Api.Swazy.Attributes;
using Api.Swazy.Common;
using Api.Swazy.Models.DTOs.Authentication;
using Api.Swazy.Models.DTOs.Users;
using Api.Swazy.Models.Entities;
using Api.Swazy.Persistence;
using Api.Swazy.Providers;
using Api.Swazy.Types;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace Api.Swazy.Modules;

public static class AuthModule
{
    public static void MapAuthEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost($"api/{SwazyConstants.AuthModuleApi}/login", async (
                [FromServices] SwazyDbContext db,
                [FromServices] IHashingProvider hashingProvider,
                [FromServices] IJwtTokenProvider jwtTokenProvider,
                [FromBody] LoginUserDto loginUserDto) =>
            {
                Log.Verbose("[AuthModule - Login] Invoked. {UserEmail}", loginUserDto.Email);

                try
                {
                    var user = await db.Users
                        .Include(u => u.BusinessAccesses)
                        .SingleOrDefaultAsync(x => x.Email == loginUserDto.Email);

                    if (user == null)
                    {
                        Log.Debug("[AuthModule - Login] User not found. {UserEmail}", loginUserDto.Email);
                        return Results.BadRequest("Invalid credentials.");
                    }

                    if (!user.IsPasswordSet)
                    {
                        Log.Debug("[AuthModule - Login] Password not set. {UserEmail} {UserId}",
                            loginUserDto.Email, user.Id);
                        return Results.BadRequest("Invalid credentials.");
                    }

                    Log.Debug("[AuthModule - Login] User found, validating password. {UserEmail} {UserId}",
                        loginUserDto.Email, user.Id);

                    var isValidPassword = hashingProvider.ValidatePassword(
                        loginUserDto.Password,
                        user.HashedPassword);

                    if (!isValidPassword)
                    {
                        Log.Debug("[AuthModule - Login] Invalid password. {UserEmail} {UserId}",
                            loginUserDto.Email, user.Id);
                        return Results.BadRequest("Invalid credentials.");
                    }

                    var primaryBusinessAccess = user.BusinessAccesses.FirstOrDefault();

                    var tokenDto = new TokenDto
                    {
                        Id = user.Id,
                        FirstName = user.FirstName,
                        LastName = user.LastName,
                        Email = user.Email,
                        SystemRole = user.SystemRole,
                        CurrentBusinessId = primaryBusinessAccess?.BusinessId,
                        CurrentBusinessRole = primaryBusinessAccess?.Role
                    };

                    var accessToken = jwtTokenProvider.GenerateAccessToken(tokenDto);
                    var refreshToken = jwtTokenProvider.GenerateRefreshToken();

                    user.RefreshToken = refreshToken;
                    user.RefreshTokenExpiresAt = DateTime.UtcNow.AddDays(SwazyConstants.JwtRefreshTokenLifetimeDays);
                    await db.SaveChangesAsync();

                    var response = new AuthResponse(
                        accessToken,
                        refreshToken,
                        SwazyConstants.JwtAccessTokenLifetimeMinutes * 60,
                        new UserInfo(user.Id, user.FirstName, user.LastName, user.Email, user.SystemRole.ToString())
                    );

                    Log.Information("[AuthModule - Login] User logged in successfully. {UserEmail} {UserId}",
                        loginUserDto.Email, user.Id);

                    return Results.Ok(response);
                }
                catch (Exception ex)
                {
                    Log.Error("[AuthModule - Login] Error occurred. {UserEmail} Exception: {Exception}",
                        loginUserDto.Email, ex);
                    return Results.Problem(statusCode: (int)HttpStatusCode.InternalServerError);
                }
            })
            .WithTags(SwazyConstants.AuthModuleName);

        endpoints.MapPost($"api/{SwazyConstants.AuthModuleApi}/register", async (
                [FromServices] SwazyDbContext db,
                [FromServices] IHashingProvider hashingProvider,
                [FromServices] IJwtTokenProvider jwtTokenProvider,
                [FromBody] RegisterUserDto registerUserDto) =>
            {
                Log.Verbose("[AuthModule - Register] Invoked. {UserEmail}", registerUserDto.Email);

                try
                {
                    var existingUser = await db.Users
                        .SingleOrDefaultAsync(x => x.Email == registerUserDto.Email);

                    if (existingUser != null)
                    {
                        Log.Debug("[AuthModule - Register] User already exists. {UserEmail}", 
                            registerUserDto.Email);
                        return Results.Conflict("User with this email already exists.");
                    }

                    Log.Debug("[AuthModule - Register] Creating new user. {UserEmail}", 
                        registerUserDto.Email);

                    var newUser = new User
                    {
                        FirstName = registerUserDto.FirstName,
                        LastName = registerUserDto.LastName,
                        Email = registerUserDto.Email,
                        PhoneNumber = registerUserDto.PhoneNumber,
                        HashedPassword = hashingProvider.HashPassword(registerUserDto.Password),
                        SystemRole = UserRole.User
                    };

                    db.Users.Add(newUser);
                    await db.SaveChangesAsync();

                    var tokenDto = new TokenDto
                    {
                        Id = newUser.Id,
                        FirstName = newUser.FirstName,
                        LastName = newUser.LastName
                    };

                    var token = jwtTokenProvider.GenerateAccessToken(tokenDto);

                    Log.Debug("[AuthModule - Register] Successfully registered user. {UserEmail} {UserId}",
                        registerUserDto.Email, newUser.Id);

                    return Results.Ok(token);
                }
                catch (Exception ex)
                {
                    Log.Error("[AuthModule - Register] Error occurred. {UserEmail} Exception: {Exception}",
                        registerUserDto.Email, ex);
                    return Results.Problem(statusCode: (int)HttpStatusCode.InternalServerError);
                }
            })
            .WithTags(SwazyConstants.AuthModuleName);

        endpoints.MapPost($"api/{SwazyConstants.AuthModuleApi}/refresh", async (
                [FromServices] SwazyDbContext db,
                [FromServices] IJwtTokenProvider jwtTokenProvider,
                [FromBody] RefreshTokenRequest request) =>
            {
                Log.Verbose("[AuthModule - Refresh] Invoked");

                try
                {
                    var user = await db.Users
                        .Include(u => u.BusinessAccesses)
                        .SingleOrDefaultAsync(u => u.RefreshToken == request.RefreshToken);

                    if (user == null || user.RefreshToken != request.RefreshToken)
                    {
                        Log.Debug("[AuthModule - Refresh] Invalid refresh token");
                        return Results.BadRequest("Invalid refresh token.");
                    }

                    if (user.RefreshTokenExpiresAt == null || user.RefreshTokenExpiresAt < DateTime.UtcNow)
                    {
                        Log.Debug("[AuthModule - Refresh] Refresh token expired. {UserId}", user.Id);
                        return Results.BadRequest("Refresh token expired.");
                    }

                    var primaryBusinessAccess = user.BusinessAccesses.FirstOrDefault();

                    var tokenDto = new TokenDto
                    {
                        Id = user.Id,
                        FirstName = user.FirstName,
                        LastName = user.LastName,
                        Email = user.Email,
                        SystemRole = user.SystemRole,
                        CurrentBusinessId = primaryBusinessAccess?.BusinessId,
                        CurrentBusinessRole = primaryBusinessAccess?.Role
                    };

                    var accessToken = jwtTokenProvider.GenerateAccessToken(tokenDto);
                    var newRefreshToken = jwtTokenProvider.GenerateRefreshToken();

                    user.RefreshToken = newRefreshToken;
                    user.RefreshTokenExpiresAt = DateTime.UtcNow.AddDays(SwazyConstants.JwtRefreshTokenLifetimeDays);
                    await db.SaveChangesAsync();

                    var response = new AuthResponse(
                        accessToken,
                        newRefreshToken,
                        SwazyConstants.JwtAccessTokenLifetimeMinutes * 60,
                        new UserInfo(user.Id, user.FirstName, user.LastName, user.Email, user.SystemRole.ToString())
                    );

                    Log.Debug("[AuthModule - Refresh] Token refreshed. {UserId}", user.Id);

                    return Results.Ok(response);
                }
                catch (Exception ex)
                {
                    Log.Error("[AuthModule - Refresh] Error occurred. Exception: {Exception}", ex);
                    return Results.Problem(statusCode: (int)HttpStatusCode.InternalServerError);
                }
            })
            .WithTags(SwazyConstants.AuthModuleName);

        endpoints.MapPost($"api/{SwazyConstants.AuthModuleApi}/logout", async (
                [FromServices] SwazyDbContext db,
                [FromBody] RefreshTokenRequest request) =>
            {
                Log.Verbose("[AuthModule - Logout] Invoked");

                try
                {
                    var user = await db.Users
                        .SingleOrDefaultAsync(u => u.RefreshToken == request.RefreshToken);

                    if (user != null)
                    {
                        user.RefreshToken = null;
                        user.RefreshTokenExpiresAt = null;
                        await db.SaveChangesAsync();

                        Log.Information("[AuthModule - Logout] User logged out. {UserId}", user.Id);
                    }

                    return Results.Ok(new { success = true });
                }
                catch (Exception ex)
                {
                    Log.Error("[AuthModule - Logout] Error occurred. Exception: {Exception}", ex);
                    return Results.Problem(statusCode: (int)HttpStatusCode.InternalServerError);
                }
            })
            .WithTags(SwazyConstants.AuthModuleName);

        endpoints.MapPost($"api/{SwazyConstants.AuthModuleApi}/setup-password", async (
                [FromServices] SwazyDbContext db,
                [FromServices] IHashingProvider hashingProvider,
                [FromServices] IJwtTokenProvider jwtTokenProvider,
                [FromBody] SetupPasswordRequest request) =>
            {
                Log.Verbose("[AuthModule - SetupPassword] Invoked");

                try
                {
                    var user = await db.Users
                        .Include(u => u.BusinessAccesses)
                        .SingleOrDefaultAsync(u => u.InvitationToken == request.InvitationToken);

                    if (user == null)
                    {
                        Log.Debug("[AuthModule - SetupPassword] Invalid invitation token");
                        return Results.BadRequest("Invalid invitation token.");
                    }

                    if (user.InvitationExpiresAt == null || user.InvitationExpiresAt < DateTime.UtcNow)
                    {
                        Log.Debug("[AuthModule - SetupPassword] Invitation token expired. {UserId}", user.Id);
                        return Results.BadRequest("Invitation token expired.");
                    }

                    if (user.IsPasswordSet)
                    {
                        Log.Debug("[AuthModule - SetupPassword] Password already set. {UserId}", user.Id);
                        return Results.BadRequest("Password already set.");
                    }

                    user.HashedPassword = hashingProvider.HashPassword(request.Password);
                    user.IsPasswordSet = true;
                    user.InvitationToken = null;
                    user.InvitationExpiresAt = null;

                    var primaryBusinessAccess = user.BusinessAccesses.FirstOrDefault();

                    var tokenDto = new TokenDto
                    {
                        Id = user.Id,
                        FirstName = user.FirstName,
                        LastName = user.LastName,
                        Email = user.Email,
                        SystemRole = user.SystemRole,
                        CurrentBusinessId = primaryBusinessAccess?.BusinessId,
                        CurrentBusinessRole = primaryBusinessAccess?.Role
                    };

                    var accessToken = jwtTokenProvider.GenerateAccessToken(tokenDto);
                    var refreshToken = jwtTokenProvider.GenerateRefreshToken();

                    user.RefreshToken = refreshToken;
                    user.RefreshTokenExpiresAt = DateTime.UtcNow.AddDays(SwazyConstants.JwtRefreshTokenLifetimeDays);
                    await db.SaveChangesAsync();

                    var response = new AuthResponse(
                        accessToken,
                        refreshToken,
                        SwazyConstants.JwtAccessTokenLifetimeMinutes * 60,
                        new UserInfo(user.Id, user.FirstName, user.LastName, user.Email, user.SystemRole.ToString())
                    );

                    Log.Information("[AuthModule - SetupPassword] Password set successfully. {UserId}", user.Id);

                    return Results.Ok(response);
                }
                catch (Exception ex)
                {
                    Log.Error("[AuthModule - SetupPassword] Error occurred. Exception: {Exception}", ex);
                    return Results.Problem(statusCode: (int)HttpStatusCode.InternalServerError);
                }
            })
            .WithTags(SwazyConstants.AuthModuleName);
    }
}