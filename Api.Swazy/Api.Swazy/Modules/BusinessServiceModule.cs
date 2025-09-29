using Api.Swazy.Common;
using Api.Swazy.Models.DTOs.BusinessServices;
using Api.Swazy.Models.Entities;
using Api.Swazy.Models.Responses;
using Api.Swazy.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Serilog;
using System.Net;

namespace Api.Swazy.Modules;

public static class BusinessServiceModule
{
    public static void MapBusinessServiceEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost($"api/{SwazyConstants.BusinessServiceModuleApi}", async (
                [FromServices] SwazyDbContext db,
                [FromBody] CreateBusinessServiceDto createDto) =>
            {
                Log.Verbose("[BusinessServiceModule - Create] Invoked.");

                try
                {
                    var businessService = new BusinessService
                    {
                        BusinessId = createDto.BusinessId,
                        ServiceId = createDto.ServiceId,
                        Price = createDto.Price,
                        Duration = (ushort)createDto.Duration
                    };

                    db.BusinessServices.Add(businessService);
                    await db.SaveChangesAsync();

                    Log.Debug("[BusinessServiceModule - Create] Successfully created. {BusinessServiceId}", 
                        businessService.Id);

                    var response = new BusinessServiceResponse(
                        businessService.Id,
                        businessService.BusinessId,
                        businessService.ServiceId,
                        businessService.Price,
                        businessService.Duration,
                        businessService.CreatedAt
                    );

                    return Results.Ok(response);
                }
                catch (Exception ex)
                {
                    Log.Error("[BusinessServiceModule - Create] Error occurred. Exception: {Exception}", ex);
                    return Results.Problem(statusCode: (int)HttpStatusCode.InternalServerError);
                }
            })
            .WithTags(SwazyConstants.BusinessServiceModuleName);

        endpoints.MapGet($"api/{SwazyConstants.BusinessServiceModuleApi}/business/{{businessId:guid}}", async (
                [FromServices] SwazyDbContext db,
                [FromRoute] Guid businessId) =>
            {
                Log.Verbose("[BusinessServiceModule - GetByBusinessId] Invoked. {BusinessId}", businessId);

                try
                {
                    var businessServices = await db.BusinessServices
                        .Where(bs => bs.BusinessId == businessId)
                        .ToListAsync();

                    if (!businessServices.Any())
                    {
                        Log.Debug("[BusinessServiceModule - GetByBusinessId] No services found. {BusinessId}", 
                            businessId);
                        return Results.NotFound("No business services found for the specified business ID.");
                    }

                    var response = businessServices.Select(bs => new BusinessServiceResponse(
                        bs.Id,
                        bs.BusinessId,
                        bs.ServiceId,
                        bs.Price,
                        bs.Duration,
                        bs.CreatedAt
                    )).ToList();

                    Log.Debug("[BusinessServiceModule - GetByBusinessId] Returned {Count} services. {BusinessId}", 
                        response.Count, businessId);

                    return Results.Ok(response);
                }
                catch (Exception ex)
                {
                    Log.Error("[BusinessServiceModule - GetByBusinessId] Error occurred. {BusinessId} Exception: {Exception}", 
                        businessId, ex);
                    return Results.Problem(statusCode: (int)HttpStatusCode.InternalServerError);
                }
            })
            .WithTags(SwazyConstants.BusinessServiceModuleName);

        endpoints.MapGet($"api/{SwazyConstants.BusinessServiceModuleApi}/all", async (
                [FromServices] SwazyDbContext db) =>
            {
                Log.Verbose("[BusinessServiceModule - GetAll] Invoked.");

                try
                {
                    var businessServices = await db.BusinessServices.ToListAsync();

                    var response = businessServices.Select(bs => new BusinessServiceResponse(
                        bs.Id,
                        bs.BusinessId,
                        bs.ServiceId,
                        bs.Price,
                        bs.Duration,
                        bs.CreatedAt
                    )).ToList();

                    Log.Debug("[BusinessServiceModule - GetAll] Returned {Count} services.", response.Count);

                    return Results.Ok(response);
                }
                catch (Exception ex)
                {
                    Log.Error("[BusinessServiceModule - GetAll] Error occurred. Exception: {Exception}", ex);
                    return Results.Problem(statusCode: (int)HttpStatusCode.InternalServerError);
                }
            })
            .WithTags(SwazyConstants.BusinessServiceModuleName);

        endpoints.MapGet($"api/{SwazyConstants.BusinessServiceModuleApi}/{{id:guid}}", async (
                [FromServices] SwazyDbContext db,
                [FromRoute] Guid id) =>
            {
                Log.Verbose("[BusinessServiceModule - GetById] Invoked. {BusinessServiceId}", id);

                try
                {
                    var businessService = await db.BusinessServices.FindAsync(id);

                    if (businessService == null)
                    {
                        Log.Debug("[BusinessServiceModule - GetById] Not found. {BusinessServiceId}", id);
                        return Results.NotFound("BusinessService not found.");
                    }

                    var response = new BusinessServiceResponse(
                        businessService.Id,
                        businessService.BusinessId,
                        businessService.ServiceId,
                        businessService.Price,
                        businessService.Duration,
                        businessService.CreatedAt
                    );

                    Log.Debug("[BusinessServiceModule - GetById] Successfully returned. {BusinessServiceId}", id);

                    return Results.Ok(response);
                }
                catch (Exception ex)
                {
                    Log.Error("[BusinessServiceModule - GetById] Error occurred. {BusinessServiceId} Exception: {Exception}", 
                        id, ex);
                    return Results.Problem(statusCode: (int)HttpStatusCode.InternalServerError);
                }
            })
            .WithTags(SwazyConstants.BusinessServiceModuleName);

        endpoints.MapPut($"api/{SwazyConstants.BusinessServiceModuleApi}", async (
                [FromServices] SwazyDbContext db,
                [FromBody] UpdateBusinessServiceDto updateDto) =>
            {
                Log.Verbose("[BusinessServiceModule - Update] Invoked. {BusinessServiceId}", updateDto.Id);

                try
                {
                    var businessService = await db.BusinessServices.FindAsync(updateDto.Id);

                    if (businessService == null)
                    {
                        Log.Debug("[BusinessServiceModule - Update] Not found. {BusinessServiceId}", updateDto.Id);
                        return Results.NotFound("BusinessService not found.");
                    }

                    if (updateDto.Price.HasValue)
                        businessService.Price = updateDto.Price.Value;

                    if (updateDto.Duration.HasValue)
                        businessService.Duration = (ushort)updateDto.Duration.Value;

                    await db.SaveChangesAsync();

                    Log.Debug("[BusinessServiceModule - Update] Successfully updated. {BusinessServiceId}", 
                        businessService.Id);

                    var response = new BusinessServiceResponse(
                        businessService.Id,
                        businessService.BusinessId,
                        businessService.ServiceId,
                        businessService.Price,
                        businessService.Duration,
                        businessService.CreatedAt
                    );

                    return Results.Ok(response);
                }
                catch (Exception ex)
                {
                    Log.Error("[BusinessServiceModule - Update] Error occurred. {BusinessServiceId} Exception: {Exception}", 
                        updateDto.Id, ex);
                    return Results.Problem(statusCode: (int)HttpStatusCode.InternalServerError);
                }
            })
            .WithTags(SwazyConstants.BusinessServiceModuleName);

        endpoints.MapDelete($"api/{SwazyConstants.BusinessServiceModuleApi}/{{id:guid}}", async (
                [FromServices] SwazyDbContext db,
                [FromRoute] Guid id) =>
            {
                Log.Verbose("[BusinessServiceModule - Delete] Invoked. {BusinessServiceId}", id);

                try
                {
                    var businessService = await db.BusinessServices.FindAsync(id);

                    if (businessService == null)
                    {
                        Log.Debug("[BusinessServiceModule - Delete] Not found. {BusinessServiceId}", id);
                        return Results.NotFound("BusinessService not found.");
                    }

                    businessService.IsDeleted = true;
                    businessService.DeletedAt = DateTimeOffset.UtcNow;

                    await db.SaveChangesAsync();

                    Log.Debug("[BusinessServiceModule - Delete] Successfully soft deleted. {BusinessServiceId}", id);

                    var response = new BusinessServiceResponse(
                        businessService.Id,
                        businessService.BusinessId,
                        businessService.ServiceId,
                        businessService.Price,
                        businessService.Duration,
                        businessService.CreatedAt
                    );

                    return Results.Ok(response);
                }
                catch (Exception ex)
                {
                    Log.Error("[BusinessServiceModule - Delete] Error occurred. {BusinessServiceId} Exception: {Exception}", 
                        id, ex);
                    return Results.Problem(statusCode: (int)HttpStatusCode.InternalServerError);
                }
            })
            .WithTags(SwazyConstants.BusinessServiceModuleName);
    }
}