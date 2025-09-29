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
        endpoints.MapPost($"api/{SwazyConstants.BusinessModuleApi}", async (
                [FromServices] SwazyDbContext db,
                [FromBody] CreateBusinessDto createBusinessDto) =>
            {
                Log.Verbose("[BusinessModule - Create] Invoked.");

                try
                {
                    var business = new Business
                    {
                        Name = createBusinessDto.Name,
                        Address = createBusinessDto.Address,
                        PhoneNumber = createBusinessDto.PhoneNumber,
                        Email = createBusinessDto.Email,
                        BusinessType = createBusinessDto.BusinessType,
                        WebsiteUrl = createBusinessDto.WebsiteUrl
                    };

                    db.Businesses.Add(business);
                    await db.SaveChangesAsync();

                    Log.Debug("[BusinessModule - Create] Successfully created. {BusinessId}", business.Id);

                    var response = new BusinessResponse(
                        business.Id,
                        business.Name,
                        business.Address,
                        business.PhoneNumber,
                        business.Email,
                        business.BusinessType.ToString(),
                        new List<BusinessEmployeeResponse>(),
                        business.WebsiteUrl,
                        business.CreatedAt
                    );

                    return Results.Ok(response);
                }
                catch (Exception ex)
                {
                    Log.Error("[BusinessModule - Create] Error occurred. Exception: {Exception}", ex);
                    return Results.Problem(statusCode: (int)HttpStatusCode.InternalServerError);
                }
            })
            .WithTags(SwazyConstants.BusinessModuleName);

        endpoints.MapGet($"api/{SwazyConstants.BusinessModuleApi}/all", async (
                [FromServices] SwazyDbContext db) =>
            {
                Log.Verbose("[BusinessModule - GetAll] Invoked.");

                try
                {
                    var businesses = await db.Businesses
                        .Include(b => b.UserAccesses)
                            .ThenInclude(ua => ua.User)
                        .ToListAsync();

                    var response = businesses.Select(b => new BusinessResponse(
                        b.Id,
                        b.Name,
                        b.Address,
                        b.PhoneNumber,
                        b.Email,
                        b.BusinessType.ToString(),
                        b.UserAccesses.Select(ua => new BusinessEmployeeResponse(
                            ua.UserId,
                            ua.User.FirstName,
                            ua.User.LastName,
                            ua.User.Email,
                            ua.Role.ToString()
                        )).ToList(),
                        b.WebsiteUrl,
                        b.CreatedAt
                    )).ToList();

                    Log.Debug("[BusinessModule - GetAll] Returned {Count} businesses.", response.Count);

                    return Results.Ok(response);
                }
                catch (Exception ex)
                {
                    Log.Error("[BusinessModule - GetAll] Error occurred. Exception: {Exception}", ex);
                    return Results.Problem(statusCode: (int)HttpStatusCode.InternalServerError);
                }
            })
            .WithTags(SwazyConstants.BusinessModuleName);

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
                    business.BusinessType = updateBusinessDto.BusinessType;
                    business.WebsiteUrl = updateBusinessDto.WebsiteUrl;

                    await db.SaveChangesAsync();

                    Log.Debug("[BusinessModule - Update] Successfully updated. {BusinessId}", business.Id);

                    await db.Entry(business)
                        .Collection(b => b.UserAccesses)
                        .Query()
                        .Include(ua => ua.User)
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

        endpoints.MapPut($"api/{SwazyConstants.BusinessModuleApi}/add-employee", async (
                [FromServices] SwazyDbContext db,
                [FromBody] AddEmployeeToBusinessDto addEmployeeDto) =>
            {
                Log.Verbose("[BusinessModule - AddEmployee] Invoked. {BusinessId}", addEmployeeDto.BusinessId);

                try
                {
                    var business = await db.Businesses.FindAsync(addEmployeeDto.BusinessId);

                    if (business == null)
                    {
                        Log.Debug("[BusinessModule - AddEmployee] Business not found. {BusinessId}",
                            addEmployeeDto.BusinessId);
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
                                                 && uba.BusinessId == addEmployeeDto.BusinessId);

                    if (existingAccess != null)
                    {
                        Log.Debug("[BusinessModule - AddEmployee] User already has access. {UserId} {BusinessId}",
                            user.Id, addEmployeeDto.BusinessId);
                        return Results.BadRequest("Employee already included in business.");
                    }

                    var userAccess = new UserBusinessAccess
                    {
                        UserId = user.Id,
                        BusinessId = addEmployeeDto.BusinessId,
                        Role = addEmployeeDto.Role
                    };

                    db.UserBusinessAccesses.Add(userAccess);
                    await db.SaveChangesAsync();

                    Log.Debug("[BusinessModule - AddEmployee] Successfully added employee. {UserId} {BusinessId}",
                        user.Id, addEmployeeDto.BusinessId);

                    await db.Entry(business)
                        .Collection(b => b.UserAccesses)
                        .Query()
                        .Include(ua => ua.User)
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
                        business.WebsiteUrl,
                        business.CreatedAt
                    );

                    return Results.Ok(response);
                }
                catch (Exception ex)
                {
                    Log.Error("[BusinessModule - AddEmployee] Error occurred. {BusinessId} Exception: {Exception}",
                        addEmployeeDto.BusinessId, ex);
                    return Results.Problem(statusCode: (int)HttpStatusCode.InternalServerError);
                }
            })
            .WithTags(SwazyConstants.BusinessModuleName);

        endpoints.MapDelete($"api/{SwazyConstants.BusinessModuleApi}/{{id:guid}}", async (
                [FromServices] SwazyDbContext db,
                [FromRoute] Guid id) =>
            {
                Log.Verbose("[BusinessModule - Delete] Invoked. {BusinessId}", id);

                try
                {
                    var business = await db.Businesses.FindAsync(id);

                    if (business == null)
                    {
                        Log.Debug("[BusinessModule - Delete] Not found. {BusinessId}", id);
                        return Results.NotFound("Business not found.");
                    }

                    business.IsDeleted = true;
                    business.DeletedAt = DateTimeOffset.UtcNow;

                    await db.SaveChangesAsync();

                    Log.Debug("[BusinessModule - Delete] Successfully soft deleted. {BusinessId}", id);

                    var response = new BusinessResponse(
                        business.Id,
                        business.Name,
                        business.Address,
                        business.PhoneNumber,
                        business.Email,
                        business.BusinessType.ToString(),
                        [],
                        business.WebsiteUrl,
                        business.CreatedAt
                    );

                    return Results.Ok(response);
                }
                catch (Exception ex)
                {
                    Log.Error("[BusinessModule - Delete] Error occurred. {BusinessId} Exception: {Exception}",
                        id, ex);
                    return Results.Problem(statusCode: (int)HttpStatusCode.InternalServerError);
                }
            })
            .WithTags(SwazyConstants.BusinessModuleName);
    }
}