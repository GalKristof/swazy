using System.Net;
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
                        .SingleOrDefaultAsync(x => x.Email == loginUserDto.Email);

                    if (user == null)
                    {
                        Log.Debug("[AuthModule - Login] User not found. {UserEmail}", loginUserDto.Email);
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

                    var tokenDto = new TokenDto
                    {
                        Id = user.Id,
                        FirstName = user.FirstName,
                        LastName = user.LastName
                    };

                    var token = jwtTokenProvider.GenerateAccessToken(tokenDto);

                    Log.Debug("[AuthModule - Login] Successfully created token. {UserEmail} {UserId}",
                        loginUserDto.Email, user.Id);

                    return Results.Ok(token);
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
    }
}