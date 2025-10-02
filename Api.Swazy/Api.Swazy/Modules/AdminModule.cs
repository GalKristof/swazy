using Api.Swazy.Common;
using Api.Swazy.Models.DTOs.Businesses;
using Api.Swazy.Models.DTOs.Services;
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

public static class AdminModule
{
    public static void MapAdminEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet($"api/admin/{SwazyConstants.BusinessModuleApi}/all", async (
                [FromServices] SwazyDbContext db) =>
            {
                Log.Verbose("[AdminModule - GetAllBusinesses] Invoked.");

                try
                {
                    var businesses = await db.Businesses
                        .Include(b => b.UserAccesses)
                            .ThenInclude(ua => ua.User)
                        .Include(b => b.Services)
                            .ThenInclude(s => s.Service)
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
                        b.Services.Select(s => new BusinessServiceResponse(
                            s.Id,
                            s.BusinessId,
                            s.ServiceId,
                            s.Service.Value,
                            s.Price,
                            s.Duration,
                            s.CreatedAt
                        )).ToList(),
                        b.WebsiteUrl,
                        b.CreatedAt
                    )).ToList();

                    Log.Debug("[AdminModule - GetAllBusinesses] Returned {Count} businesses.", response.Count);

                    return Results.Ok(response);
                }
                catch (Exception ex)
                {
                    Log.Error("[AdminModule - GetAllBusinesses] Error occurred. Exception: {Exception}", ex);
                    return Results.Problem(statusCode: (int)HttpStatusCode.InternalServerError);
                }
            })
            .WithTags("Admin");

        endpoints.MapPost($"api/admin/{SwazyConstants.BusinessModuleApi}", async (
                [FromServices] SwazyDbContext db,
                [FromBody] CreateBusinessDto createBusinessDto) =>
            {
                Log.Verbose("[AdminModule - CreateBusiness] Invoked.");

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

                    Log.Debug("[AdminModule - CreateBusiness] Successfully created. {BusinessId}", business.Id);

                    var response = new BusinessResponse(
                        business.Id,
                        business.Name,
                        business.Address,
                        business.PhoneNumber,
                        business.Email,
                        business.BusinessType.ToString(),
                        new List<BusinessEmployeeResponse>(),
                        new List<BusinessServiceResponse>(),
                        business.WebsiteUrl,
                        business.CreatedAt
                    );

                    return Results.Ok(response);
                }
                catch (Exception ex)
                {
                    Log.Error("[AdminModule - CreateBusiness] Error occurred. Exception: {Exception}", ex);
                    return Results.Problem(statusCode: (int)HttpStatusCode.InternalServerError);
                }
            })
            .WithTags("Admin");

        endpoints.MapDelete($"api/admin/{SwazyConstants.BusinessModuleApi}/{{id:guid}}", async (
                [FromServices] SwazyDbContext db,
                [FromRoute] Guid id) =>
            {
                Log.Verbose("[AdminModule - DeleteBusiness] Invoked. {BusinessId}", id);

                try
                {
                    var business = await db.Businesses.FindAsync(id);

                    if (business == null)
                    {
                        Log.Debug("[AdminModule - DeleteBusiness] Not found. {BusinessId}", id);
                        return Results.NotFound("Business not found.");
                    }

                    business.IsDeleted = true;
                    business.DeletedAt = DateTimeOffset.UtcNow;

                    await db.SaveChangesAsync();

                    Log.Debug("[AdminModule - DeleteBusiness] Successfully soft deleted. {BusinessId}", id);

                    var response = new BusinessResponse(
                        business.Id,
                        business.Name,
                        business.Address,
                        business.PhoneNumber,
                        business.Email,
                        business.BusinessType.ToString(),
                        [],
                        [],
                        business.WebsiteUrl,
                        business.CreatedAt
                    );

                    return Results.Ok(response);
                }
                catch (Exception ex)
                {
                    Log.Error("[AdminModule - DeleteBusiness] Error occurred. {BusinessId} Exception: {Exception}",
                        id, ex);
                    return Results.Problem(statusCode: (int)HttpStatusCode.InternalServerError);
                }
            })
            .WithTags("Admin");

        endpoints.MapPut($"api/admin/{SwazyConstants.BusinessModuleApi}/{{id:guid}}/type", async (
                [FromServices] SwazyDbContext db,
                [FromRoute] Guid id,
                [FromBody] UpdateBusinessTypeDto updateDto) =>
            {
                Log.Verbose("[AdminModule - UpdateBusinessType] Invoked. {BusinessId}", id);

                try
                {
                    var business = await db.Businesses.FindAsync(id);

                    if (business == null)
                    {
                        Log.Debug("[AdminModule - UpdateBusinessType] Not found. {BusinessId}", id);
                        return Results.NotFound("Business not found.");
                    }

                    business.BusinessType = updateDto.BusinessType;

                    await db.SaveChangesAsync();

                    Log.Debug("[AdminModule - UpdateBusinessType] Successfully updated. {BusinessId} {NewType}",
                        id, updateDto.BusinessType);

                    return Results.Ok(new { BusinessId = id, BusinessType = business.BusinessType.ToString() });
                }
                catch (Exception ex)
                {
                    Log.Error("[AdminModule - UpdateBusinessType] Error occurred. {BusinessId} Exception: {Exception}",
                        id, ex);
                    return Results.Problem(statusCode: (int)HttpStatusCode.InternalServerError);
                }
            })
            .WithTags("Admin");

        endpoints.MapGet($"api/admin/{SwazyConstants.UserModuleApi}/all", async (
                [FromServices] SwazyDbContext db) =>
            {
                Log.Verbose("[AdminModule - GetAllUsers] Invoked.");

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

                    Log.Debug("[AdminModule - GetAllUsers] Returned {Count} users.", response.Count);

                    return Results.Ok(response);
                }
                catch (Exception ex)
                {
                    Log.Error("[AdminModule - GetAllUsers] Error occurred. Exception: {Exception}", ex);
                    return Results.Problem(statusCode: (int)HttpStatusCode.InternalServerError);
                }
            })
            .WithTags("Admin");

        endpoints.MapPost($"api/admin/{SwazyConstants.UserModuleApi}", async (
                [FromServices] SwazyDbContext db,
                [FromServices] IHashingProvider hashingProvider,
                [FromBody] CreateUserDto createUserDto) =>
            {
                Log.Verbose("[AdminModule - CreateUser] Invoked. {UserEmail}", createUserDto.Email);

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

                    Log.Debug("[AdminModule - CreateUser] Successfully created. {UserEmail} {UserId}",
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
                    Log.Error("[AdminModule - CreateUser] Error occurred. {UserEmail} Exception: {Exception}",
                        createUserDto.Email, ex);
                    return Results.Problem(statusCode: (int)HttpStatusCode.InternalServerError);
                }
            })
            .WithTags("Admin");

        endpoints.MapPut($"api/admin/{SwazyConstants.UserModuleApi}/{{id:guid}}/systemrole", async (
                [FromServices] SwazyDbContext db,
                [FromRoute] Guid id,
                [FromBody] UpdateUserSystemRoleDto updateDto) =>
            {
                Log.Verbose("[AdminModule - UpdateUserSystemRole] Invoked. {UserId}", id);

                try
                {
                    var user = await db.Users.FindAsync(id);

                    if (user == null)
                    {
                        Log.Debug("[AdminModule - UpdateUserSystemRole] Not found. {UserId}", id);
                        return Results.NotFound("User not found.");
                    }

                    user.SystemRole = updateDto.SystemRole;

                    await db.SaveChangesAsync();

                    Log.Debug("[AdminModule - UpdateUserSystemRole] Successfully updated. {UserId} {NewRole}",
                        id, updateDto.SystemRole);

                    return Results.Ok(new { UserId = id, SystemRole = user.SystemRole.ToString() });
                }
                catch (Exception ex)
                {
                    Log.Error("[AdminModule - UpdateUserSystemRole] Error occurred. {UserId} Exception: {Exception}",
                        id, ex);
                    return Results.Problem(statusCode: (int)HttpStatusCode.InternalServerError);
                }
            })
            .WithTags("Admin");

        endpoints.MapDelete($"api/admin/{SwazyConstants.UserModuleApi}/{{id:guid}}", async (
                [FromServices] SwazyDbContext db,
                [FromRoute] Guid id) =>
            {
                Log.Verbose("[AdminModule - DeleteUser] Invoked. {UserId}", id);

                try
                {
                    var user = await db.Users.FindAsync(id);

                    if (user == null)
                    {
                        Log.Debug("[AdminModule - DeleteUser] Not found. {UserId}", id);
                        return Results.NotFound("User not found.");
                    }

                    user.IsDeleted = true;
                    user.DeletedAt = DateTimeOffset.UtcNow;

                    await db.SaveChangesAsync();

                    Log.Debug("[AdminModule - DeleteUser] Successfully soft deleted. {UserId}", id);

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
                    Log.Error("[AdminModule - DeleteUser] Error occurred. {UserId} Exception: {Exception}",
                        id, ex);
                    return Results.Problem(statusCode: (int)HttpStatusCode.InternalServerError);
                }
            })
            .WithTags("Admin");

        endpoints.MapPost($"api/admin/{SwazyConstants.ServiceModuleApi}", async (
                [FromServices] SwazyDbContext db,
                [FromBody] CreateServiceDto createServiceDto) =>
            {
                Log.Verbose("[AdminModule - CreateService] Invoked.");

                try
                {
                    var service = new Service
                    {
                        Tag = createServiceDto.Tag,
                        BusinessType = createServiceDto.BusinessType,
                        Value = createServiceDto.Value
                    };

                    db.Services.Add(service);
                    await db.SaveChangesAsync();

                    Log.Debug("[AdminModule - CreateService] Successfully created. {ServiceId}", service.Id);

                    var response = new ServiceResponse(
                        service.Id,
                        service.Tag,
                        service.BusinessType.ToString(),
                        service.Value,
                        service.CreatedAt
                    );

                    return Results.Ok(response);
                }
                catch (Exception ex)
                {
                    Log.Error("[AdminModule - CreateService] Error occurred. Exception: {Exception}", ex);
                    return Results.Problem(statusCode: (int)HttpStatusCode.InternalServerError);
                }
            })
            .WithTags("Admin");

        endpoints.MapGet($"api/admin/{SwazyConstants.ServiceModuleApi}/all", async (
                [FromServices] SwazyDbContext db) =>
            {
                Log.Verbose("[AdminModule - GetAllServices] Invoked.");

                try
                {
                    var services = await db.Services.ToListAsync();

                    var response = services.Select(s => new ServiceResponse(
                        s.Id,
                        s.Tag,
                        s.BusinessType.ToString(),
                        s.Value,
                        s.CreatedAt
                    )).ToList();

                    Log.Debug("[AdminModule - GetAllServices] Returned {Count} services.", response.Count);

                    return Results.Ok(response);
                }
                catch (Exception ex)
                {
                    Log.Error("[AdminModule - GetAllServices] Error occurred. Exception: {Exception}", ex);
                    return Results.Problem(statusCode: (int)HttpStatusCode.InternalServerError);
                }
            })
            .WithTags("Admin");

        endpoints.MapPut($"api/admin/{SwazyConstants.ServiceModuleApi}", async (
                [FromServices] SwazyDbContext db,
                [FromBody] UpdateServiceDto updateServiceDto) =>
            {
                Log.Verbose("[AdminModule - UpdateService] Invoked. {ServiceId}", updateServiceDto.Id);

                try
                {
                    var service = await db.Services.FindAsync(updateServiceDto.Id);

                    if (service == null)
                    {
                        Log.Debug("[AdminModule - UpdateService] Not found. {ServiceId}", updateServiceDto.Id);
                        return Results.NotFound("Service not found.");
                    }

                    service.Tag = updateServiceDto.Tag;
                    service.BusinessType = updateServiceDto.BusinessType;
                    service.Value = updateServiceDto.Value;

                    await db.SaveChangesAsync();

                    Log.Debug("[AdminModule - UpdateService] Successfully updated. {ServiceId}", service.Id);

                    var response = new ServiceResponse(
                        service.Id,
                        service.Tag,
                        service.BusinessType.ToString(),
                        service.Value,
                        service.CreatedAt
                    );

                    return Results.Ok(response);
                }
                catch (Exception ex)
                {
                    Log.Error("[AdminModule - UpdateService] Error occurred. {ServiceId} Exception: {Exception}",
                        updateServiceDto.Id, ex);
                    return Results.Problem(statusCode: (int)HttpStatusCode.InternalServerError);
                }
            })
            .WithTags("Admin");

        endpoints.MapDelete($"api/admin/{SwazyConstants.ServiceModuleApi}/{{id:guid}}", async (
                [FromServices] SwazyDbContext db,
                [FromRoute] Guid id) =>
            {
                Log.Verbose("[AdminModule - DeleteService] Invoked. {ServiceId}", id);

                try
                {
                    var service = await db.Services.FindAsync(id);

                    if (service == null)
                    {
                        Log.Debug("[AdminModule - DeleteService] Not found. {ServiceId}", id);
                        return Results.NotFound("Service not found.");
                    }

                    service.IsDeleted = true;
                    service.DeletedAt = DateTimeOffset.UtcNow;

                    await db.SaveChangesAsync();

                    Log.Debug("[AdminModule - DeleteService] Successfully soft deleted. {ServiceId}", id);

                    return Results.NoContent();
                }
                catch (Exception ex)
                {
                    Log.Error("[AdminModule - DeleteService] Error occurred. {ServiceId} Exception: {Exception}",
                        id, ex);
                    return Results.Problem(statusCode: (int)HttpStatusCode.InternalServerError);
                }
            })
            .WithTags("Admin");

        // ========== BOOKING ADMIN ENDPOINTS ==========

        endpoints.MapGet($"api/admin/{SwazyConstants.BookingModuleApi}/all", async (
                [FromServices] SwazyDbContext db) =>
            {
                Log.Verbose("[AdminModule - GetAllBookings] Invoked.");

                try
                {
                    var bookings = await db.Bookings.ToListAsync();

                    var response = bookings.Select(b => new BookingResponse(
                        b.Id,
                        b.ConfirmationCode,
                        b.BookingDate,
                        b.Notes,
                        b.FirstName,
                        b.LastName,
                        b.Email,
                        b.PhoneNumber,
                        b.BusinessServiceId,
                        b.EmployeeId,
                        b.BookedByUserId,
                        b.CreatedAt
                    )).ToList();

                    Log.Debug("[AdminModule - GetAllBookings] Returned {Count} bookings.", response.Count);

                    return Results.Ok(response);
                }
                catch (Exception ex)
                {
                    Log.Error("[AdminModule - GetAllBookings] Error occurred. Exception: {Exception}", ex);
                    return Results.Problem(statusCode: (int)HttpStatusCode.InternalServerError);
                }
            })
            .WithTags("Admin");

        endpoints.MapGet($"api/admin/{SwazyConstants.BusinessServiceModuleApi}/all", async (
                [FromServices] SwazyDbContext db) =>
            {
                Log.Verbose("[AdminModule - GetAllBusinessServices] Invoked.");

                try
                {
                    var businessServices = await db.BusinessServices
                        .Include(bs => bs.Service)
                        .ToListAsync();

                    var response = businessServices.Select(bs => new BusinessServiceResponse(
                        bs.Id,
                        bs.BusinessId,
                        bs.ServiceId,
                        bs.Service.Value,
                        bs.Price,
                        bs.Duration,
                        bs.CreatedAt
                    )).ToList();

                    Log.Debug("[AdminModule - GetAllBusinessServices] Returned {Count} services.", response.Count);

                    return Results.Ok(response);
                }
                catch (Exception ex)
                {
                    Log.Error("[AdminModule - GetAllBusinessServices] Error occurred. Exception: {Exception}", ex);
                    return Results.Problem(statusCode: (int)HttpStatusCode.InternalServerError);
                }
            })
            .WithTags("Admin");
    }
}
