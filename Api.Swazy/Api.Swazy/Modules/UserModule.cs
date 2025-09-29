using Api.Swazy.Common;
using Api.Swazy.Models.DTOs.Users;
using Api.Swazy.Models.Entities;
using Api.Swazy.Models.Responses;
using Api.Swazy.Persistence;
using Api.Swazy.Providers;
using Api.Swazy.Types;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Serilog;
using System.Net;

namespace Api.Swazy.Modules;

public static class UserModule
{
    public static void MapUserEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost($"api/{SwazyConstants.UserModuleApi}", async (
                [FromServices] SwazyDbContext db,
                [FromServices] IHashingProvider hashingProvider,
                [FromBody] CreateUserDto createUserDto) =>
            {
                Log.Verbose("[UserModule - Create] Invoked. {UserEmail}", createUserDto.Email);

                try
                {
                    var user = new User
                    {
                        FirstName = createUserDto.FirstName,
                        LastName = createUserDto.LastName,
                        Email = createUserDto.Email,
                        PhoneNumber = createUserDto.PhoneNumber,
                        HashedPassword = hashingProvider.HashPassword(createUserDto.Password),
                        SystemRole = UserRole.User
                    };

                    db.Users.Add(user);
                    await db.SaveChangesAsync();

                    Log.Debug("[UserModule - Create] Successfully created. {UserEmail} {UserId}",
                        createUserDto.Email, user.Id);

                    var response = new UserResponse(
                        user.Id,
                        user.FirstName,
                        user.LastName,
                        user.Email,
                        user.PhoneNumber,
                        user.SystemRole.ToString(),
                        new List<UserBusinessResponse>(),
                        user.CreatedAt
                    );

                    return Results.Ok(response);
                }
                catch (Exception ex)
                {
                    Log.Error("[UserModule - Create] Error occurred. {UserEmail} Exception: {Exception}",
                        createUserDto.Email, ex);
                    return Results.Problem(statusCode: (int)HttpStatusCode.InternalServerError);
                }
            })
            .WithTags(SwazyConstants.UserModuleName);

        endpoints.MapGet($"api/{SwazyConstants.UserModuleApi}/all", async (
                [FromServices] SwazyDbContext db) =>
            {
                Log.Verbose("[UserModule - GetAll] Invoked.");

                try
                {
                    var users = await db.Users
                        .Include(u => u.BusinessAccesses)
                            .ThenInclude(ba => ba.Business)
                        .ToListAsync();

                    var response = users.Select(u => new UserResponse(
                        u.Id,
                        u.FirstName,
                        u.LastName,
                        u.Email,
                        u.PhoneNumber,
                        u.SystemRole.ToString(),
                        u.BusinessAccesses.Select(ba => new UserBusinessResponse(
                            ba.BusinessId,
                            ba.Business.Name,
                            ba.Role.ToString()
                        )).ToList(),
                        u.CreatedAt
                    )).ToList();

                    Log.Debug("[UserModule - GetAll] Returned {Count} users.", response.Count);

                    return Results.Ok(response);
                }
                catch (Exception ex)
                {
                    Log.Error("[UserModule - GetAll] Error occurred. Exception: {Exception}", ex);
                    return Results.Problem(statusCode: (int)HttpStatusCode.InternalServerError);
                }
            })
            .WithTags(SwazyConstants.UserModuleName);

        endpoints.MapGet($"api/{SwazyConstants.UserModuleApi}/{{id:guid}}", async (
                [FromServices] SwazyDbContext db,
                [FromRoute] Guid id) =>
            {
                Log.Verbose("[UserModule - GetById] Invoked. {UserId}", id);

                try
                {
                    var user = await db.Users
                        .Include(u => u.BusinessAccesses)
                            .ThenInclude(ba => ba.Business)
                        .FirstOrDefaultAsync(u => u.Id == id);

                    if (user == null)
                    {
                        Log.Debug("[UserModule - GetById] Not found. {UserId}", id);
                        return Results.NotFound("User not found.");
                    }

                    var response = new UserResponse(
                        user.Id,
                        user.FirstName,
                        user.LastName,
                        user.Email,
                        user.PhoneNumber,
                        user.SystemRole.ToString(),
                        user.BusinessAccesses.Select(ba => new UserBusinessResponse(
                            ba.BusinessId,
                            ba.Business.Name,
                            ba.Role.ToString()
                        )).ToList(),
                        user.CreatedAt
                    );

                    Log.Debug("[UserModule - GetById] Successfully returned. {UserId}", id);

                    return Results.Ok(response);
                }
                catch (Exception ex)
                {
                    Log.Error("[UserModule - GetById] Error occurred. {UserId} Exception: {Exception}",
                        id, ex);
                    return Results.Problem(statusCode: (int)HttpStatusCode.InternalServerError);
                }
            })
            .WithTags(SwazyConstants.UserModuleName);

        endpoints.MapDelete($"api/{SwazyConstants.UserModuleApi}/{{id:guid}}", async (
                [FromServices] SwazyDbContext db,
                [FromRoute] Guid id) =>
            {
                Log.Verbose("[UserModule - Delete] Invoked. {UserId}", id);

                try
                {
                    var user = await db.Users.FindAsync(id);

                    if (user == null)
                    {
                        Log.Debug("[UserModule - Delete] Not found. {UserId}", id);
                        return Results.NotFound("User not found.");
                    }

                    user.IsDeleted = true;
                    user.DeletedAt = DateTimeOffset.UtcNow;

                    await db.SaveChangesAsync();

                    Log.Debug("[UserModule - Delete] Successfully soft deleted. {UserId}", id);

                    var response = new UserResponse(
                        user.Id,
                        user.FirstName,
                        user.LastName,
                        user.Email,
                        user.PhoneNumber,
                        user.SystemRole.ToString(),
                        new List<UserBusinessResponse>(),
                        user.CreatedAt
                    );

                    return Results.Ok(response);
                }
                catch (Exception ex)
                {
                    Log.Error("[UserModule - Delete] Error occurred. {UserId} Exception: {Exception}",
                        id, ex);
                    return Results.Problem(statusCode: (int)HttpStatusCode.InternalServerError);
                }
            })
            .WithTags(SwazyConstants.UserModuleName);

        endpoints.MapPut($"api/{SwazyConstants.UserModuleApi}", async (
                [FromServices] SwazyDbContext db,
                [FromBody] UpdateUserDto updateUserDto) =>
            {
                Log.Verbose("[UserModule - Update] Invoked. {UserId}", updateUserDto.Id);

                try
                {
                    var user = await db.Users.FindAsync(updateUserDto.Id);

                    if (user == null)
                    {
                        Log.Debug("[UserModule - Update] Not found. {UserId}", updateUserDto.Id);
                        return Results.NotFound("User not found.");
                    }

                    user.FirstName = updateUserDto.FirstName;
                    user.LastName = updateUserDto.LastName;
                    user.Email = updateUserDto.Email;
                    user.PhoneNumber = updateUserDto.PhoneNumber;
                    
                    if (Enum.TryParse<UserRole>(updateUserDto.Role, out var role))
                    {
                        user.SystemRole = role;
                    }

                    await db.SaveChangesAsync();

                    Log.Debug("[UserModule - Update] Successfully updated. {UserId}", user.Id);

                    await db.Entry(user)
                        .Collection(u => u.BusinessAccesses)
                        .Query()
                        .Include(ba => ba.Business)
                        .LoadAsync();

                    var response = new UserResponse(
                        user.Id,
                        user.FirstName,
                        user.LastName,
                        user.Email,
                        user.PhoneNumber,
                        user.SystemRole.ToString(),
                        user.BusinessAccesses.Select(ba => new UserBusinessResponse(
                            ba.BusinessId,
                            ba.Business.Name,
                            ba.Role.ToString()
                        )).ToList(),
                        user.CreatedAt
                    );

                    return Results.Ok(response);
                }
                catch (Exception ex)
                {
                    Log.Error("[UserModule - Update] Error occurred. {UserId} Exception: {Exception}",
                        updateUserDto.Id, ex);
                    return Results.Problem(statusCode: (int)HttpStatusCode.InternalServerError);
                }
            })
            .WithTags(SwazyConstants.UserModuleName);
    }
}