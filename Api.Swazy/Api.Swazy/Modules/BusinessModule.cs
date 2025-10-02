using Api.Swazy.Common;
using Api.Swazy.Models.DTOs.Businesses;
using Api.Swazy.Models.Entities;
using Api.Swazy.Models.Responses;
using Api.Swazy.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Serilog;
using System.Net;

namespace Api.Swazy.Modules;

public static class BusinessModule
{
    public static void MapBusinessEndpoints(this IEndpointRouteBuilder endpoints)
    {


        endpoints.MapGet($"api/{SwazyConstants.BusinessModuleApi}/{{businessId:guid}}", async (
                [FromServices] SwazyDbContext db,
                [FromRoute] Guid businessId) =>
            {
                Log.Verbose("[BusinessModule - GetById] Invoked. {BusinessId}", businessId);

                try
                {
                    var business = await db.Businesses
                        .Include(b => b.UserAccesses)
                            .ThenInclude(ua => ua.User)
                        .Include(b => b.Services)
                            .ThenInclude(s => s.Service)
                        .FirstOrDefaultAsync(b => b.Id == businessId);

                    if (business == null)
                    {
                        Log.Debug("[BusinessModule - GetById] Not found. {BusinessId}", businessId);
                        return Results.NotFound("Business not found.");
                    }

                    var response = new BusinessResponse(
                        business.Id,
                        business.Name,
                        business.Address,
                        business.PhoneNumber,
                        business.Email,
                        business.BusinessType.ToString(),
                        business.UserAccesses.Select(ua => new BusinessEmployeeResponse(
                            ua.UserId,
                            ua.User.FirstName,
                            ua.User.LastName,
                            ua.User.Email,
                            ua.Role.ToString()
                        )).ToList(),
                        business.Services.Select(s => new BusinessServiceResponse(
                            s.Id,
                            s.BusinessId,
                            s.ServiceId,
                            s.Service.Value,
                            s.Price,
                            s.Duration,
                            s.CreatedAt
                        )).ToList(),
                        business.WebsiteUrl,
                        business.CreatedAt
                    );

                    Log.Debug("[BusinessModule - GetById] Successfully returned. {BusinessId}", businessId);

                    return Results.Ok(response);
                }
                catch (Exception ex)
                {
                    Log.Error("[BusinessModule - GetById] Error occurred. {BusinessId} Exception: {Exception}",
                        businessId, ex);
                    return Results.Problem(statusCode: (int)HttpStatusCode.InternalServerError);
                }
            })
            .WithTags(SwazyConstants.BusinessModuleName);

        endpoints.MapPut($"api/{SwazyConstants.BusinessModuleApi}", async (
                [FromServices] SwazyDbContext db,
                [FromBody] UpdateBusinessDto updateBusinessDto) =>
            {
                Log.Verbose("[BusinessModule - Update] Invoked. {BusinessId}", updateBusinessDto.Id);

                try
                {
                    var business = await db.Businesses.FindAsync(updateBusinessDto.Id);

                    if (business == null)
                    {
                        Log.Debug("[BusinessModule - Update] Not found. {BusinessId}", updateBusinessDto.Id);
                        return Results.NotFound("Business not found.");
                    }

                    business.Name = updateBusinessDto.Name;
                    business.Address = updateBusinessDto.Address;
                    business.PhoneNumber = updateBusinessDto.PhoneNumber;
                    business.Email = updateBusinessDto.Email;

                    await db.SaveChangesAsync();

                    Log.Debug("[BusinessModule - Update] Successfully updated. {BusinessId}", business.Id);

                    await db.Entry(business)
                        .Collection(b => b.UserAccesses)
                        .Query()
                        .Include(ua => ua.User)
                        .LoadAsync();

                    await db.Entry(business)
                        .Collection(b => b.Services)
                        .LoadAsync();

                    var response = new BusinessResponse(
                        business.Id,
                        business.Name,
                        business.Address,
                        business.PhoneNumber,
                        business.Email,
                        business.BusinessType.ToString(),
                        business.UserAccesses.Select(ua => new BusinessEmployeeResponse(
                            ua.UserId,
                            ua.User.FirstName,
                            ua.User.LastName,
                            ua.User.Email,
                            ua.Role.ToString()
                        )).ToList(),
                        business.Services.Select(s => new BusinessServiceResponse(
                            s.Id,
                            s.BusinessId,
                            s.ServiceId,
                            s.Service.Value,
                            s.Price,
                            s.Duration,
                            s.CreatedAt
                        )).ToList(),
                        business.WebsiteUrl,
                        business.CreatedAt
                    );

                    return Results.Ok(response);
                }
                catch (Exception ex)
                {
                    Log.Error("[BusinessModule - Update] Error occurred. {BusinessId} Exception: {Exception}",
                        updateBusinessDto.Id, ex);
                    return Results.Problem(statusCode: (int)HttpStatusCode.InternalServerError);
                }
            })
            .WithTags(SwazyConstants.BusinessModuleName);

        endpoints.MapPost($"api/{SwazyConstants.BusinessModuleApi}/{{businessId:guid}}/employee", async (
                [FromServices] SwazyDbContext db,
                [FromRoute] Guid businessId,
                [FromBody] AddEmployeeToBusinessDto addEmployeeDto) =>
            {
                Log.Verbose("[BusinessModule - AddEmployee] Invoked. {BusinessId}", businessId);

                try
                {
                    var business = await db.Businesses.FindAsync(businessId);

                    if (business == null)
                    {
                        Log.Debug("[BusinessModule - AddEmployee] Business not found. {BusinessId}",
                            businessId);
                        return Results.NotFound("Business not found.");
                    }

                    var user = await db.Users
                        .SingleOrDefaultAsync(u => u.Email == addEmployeeDto.UserEmail);

                    if (user == null)
                    {
                        Log.Debug("[BusinessModule - AddEmployee] User not found. {UserEmail}",
                            addEmployeeDto.UserEmail);
                        return Results.NotFound("User not found.");
                    }

                    var existingAccess = await db.UserBusinessAccesses
                        .FirstOrDefaultAsync(uba => uba.UserId == user.Id
                                                 && uba.BusinessId == businessId);

                    if (existingAccess != null)
                    {
                        Log.Debug("[BusinessModule - AddEmployee] User already has access. {UserId} {BusinessId}",
                            user.Id, businessId);
                        return Results.BadRequest("Employee already included in business.");
                    }

                    var userAccess = new UserBusinessAccess
                    {
                        UserId = user.Id,
                        BusinessId = businessId,
                        Role = addEmployeeDto.Role
                    };

                    db.UserBusinessAccesses.Add(userAccess);
                    await db.SaveChangesAsync();

                    Log.Debug("[BusinessModule - AddEmployee] Successfully added employee. {UserId} {BusinessId}",
                        user.Id, businessId);

                    await db.Entry(business)
                        .Collection(b => b.UserAccesses)
                        .Query()
                        .Include(ua => ua.User)
                        .LoadAsync();

                    await db.Entry(business)
                        .Collection(b => b.Services)
                        .LoadAsync();

                    var response = new BusinessResponse(
                        business.Id,
                        business.Name,
                        business.Address,
                        business.PhoneNumber,
                        business.Email,
                        business.BusinessType.ToString(),
                        business.UserAccesses.Select(ua => new BusinessEmployeeResponse(
                            ua.UserId,
                            ua.User.FirstName,
                            ua.User.LastName,
                            ua.User.Email,
                            ua.Role.ToString()
                        )).ToList(),
                        business.Services.Select(s => new BusinessServiceResponse(
                            s.Id,
                            s.BusinessId,
                            s.ServiceId,
                            s.Service.Value,
                            s.Price,
                            s.Duration,
                            s.CreatedAt
                        )).ToList(),
                        business.WebsiteUrl,
                        business.CreatedAt
                    );

                    return Results.Ok(response);
                }
                catch (Exception ex)
                {
                    Log.Error("[BusinessModule - AddEmployee] Error occurred. {BusinessId} Exception: {Exception}",
                        businessId, ex);
                    return Results.Problem(statusCode: (int)HttpStatusCode.InternalServerError);
                }
            })
            .WithTags(SwazyConstants.BusinessModuleName);

        endpoints.MapPatch($"api/{SwazyConstants.BusinessModuleApi}/{{businessId:guid}}/employee/{{userId:guid}}", async (
                [FromServices] SwazyDbContext db,
                [FromRoute] Guid businessId,
                [FromRoute] Guid userId,
                [FromBody] UpdateEmployeeRoleDto updateEmployeeRoleDto) =>
            {
                Log.Verbose("[BusinessModule - UpdateEmployeeRole] Invoked. {BusinessId} {UserId}", businessId, userId);

                try
                {
                    var userAccess = await db.UserBusinessAccesses
                        .Include(uba => uba.User)
                        .FirstOrDefaultAsync(uba => uba.BusinessId == businessId && uba.UserId == userId);

                    if (userAccess == null)
                    {
                        Log.Debug("[BusinessModule - UpdateEmployeeRole] Employee not found. {BusinessId} {UserId}",
                            businessId, userId);
                        return Results.NotFound("Employee not found in business.");
                    }

                    userAccess.Role = updateEmployeeRoleDto.Role;
                    await db.SaveChangesAsync();

                    Log.Debug("[BusinessModule - UpdateEmployeeRole] Successfully updated role. {BusinessId} {UserId} {Role}",
                        businessId, userId, updateEmployeeRoleDto.Role);

                    var response = new BusinessEmployeeResponse(
                        userAccess.UserId,
                        userAccess.User.FirstName,
                        userAccess.User.LastName,
                        userAccess.User.Email,
                        userAccess.Role.ToString()
                    );

                    return Results.Ok(response);
                }
                catch (Exception ex)
                {
                    Log.Error("[BusinessModule - UpdateEmployeeRole] Error occurred. {BusinessId} {UserId} Exception: {Exception}",
                        businessId, userId, ex);
                    return Results.Problem(statusCode: (int)HttpStatusCode.InternalServerError);
                }
            })
            .WithTags(SwazyConstants.BusinessModuleName);

        endpoints.MapDelete($"api/{SwazyConstants.BusinessModuleApi}/{{businessId:guid}}/employee/{{userId:guid}}", async (
                [FromServices] SwazyDbContext db,
                [FromRoute] Guid businessId,
                [FromRoute] Guid userId) =>
            {
                Log.Verbose("[BusinessModule - RemoveEmployee] Invoked. {BusinessId} {UserId}", businessId, userId);

                try
                {
                    var userAccess = await db.UserBusinessAccesses
                        .FirstOrDefaultAsync(uba => uba.BusinessId == businessId && uba.UserId == userId);

                    if (userAccess == null)
                    {
                        Log.Debug("[BusinessModule - RemoveEmployee] Employee not found. {BusinessId} {UserId}",
                            businessId, userId);
                        return Results.NotFound("Employee not found in business.");
                    }

                    db.UserBusinessAccesses.Remove(userAccess);
                    await db.SaveChangesAsync();

                    Log.Debug("[BusinessModule - RemoveEmployee] Successfully removed employee. {BusinessId} {UserId}",
                        businessId, userId);

                    return Results.Ok("Employee removed successfully.");
                }
                catch (Exception ex)
                {
                    Log.Error("[BusinessModule - RemoveEmployee] Error occurred. {BusinessId} {UserId} Exception: {Exception}",
                        businessId, userId, ex);
                    return Results.Problem(statusCode: (int)HttpStatusCode.InternalServerError);
                }
            })
            .WithTags(SwazyConstants.BusinessModuleName);

    }
}