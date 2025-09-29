using Api.Swazy.Common;
using Api.Swazy.Models.DTOs.Services;
using Api.Swazy.Models.Entities;
using Api.Swazy.Models.Responses;
using Api.Swazy.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Serilog;
using System.Net;

namespace Api.Swazy.Modules;

public static class ServiceModule
{
    public static void MapServiceEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost($"api/{SwazyConstants.ServiceModuleApi}", async (
                [FromServices] SwazyDbContext db,
                [FromBody] CreateServiceDto createServiceDto) =>
            {
                Log.Verbose("[ServiceModule - Create] Invoked.");

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

                    Log.Debug("[ServiceModule - Create] Successfully created. {ServiceId}", service.Id);

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
                    Log.Error("[ServiceModule - Create] Error occurred. Exception: {Exception}", ex);
                    return Results.Problem(statusCode: (int)HttpStatusCode.InternalServerError);
                }
            })
            .WithTags(SwazyConstants.ServiceModuleName);

        endpoints.MapGet($"api/{SwazyConstants.ServiceModuleApi}/{{id:guid}}", async (
                [FromServices] SwazyDbContext db,
                [FromRoute] Guid id) =>
            {
                Log.Verbose("[ServiceModule - GetById] Invoked. {ServiceId}", id);

                try
                {
                    var service = await db.Services.FindAsync(id);

                    if (service == null)
                    {
                        Log.Debug("[ServiceModule - GetById] Not found. {ServiceId}", id);
                        return Results.NotFound("Service not found.");
                    }

                    var response = new ServiceResponse(
                        service.Id,
                        service.Tag,
                        service.BusinessType.ToString(),
                        service.Value,
                        service.CreatedAt
                    );

                    Log.Debug("[ServiceModule - GetById] Successfully returned. {ServiceId}", id);

                    return Results.Ok(response);
                }
                catch (Exception ex)
                {
                    Log.Error("[ServiceModule - GetById] Error occurred. {ServiceId} Exception: {Exception}",
                        id, ex);
                    return Results.Problem(statusCode: (int)HttpStatusCode.InternalServerError);
                }
            })
            .WithTags(SwazyConstants.ServiceModuleName);

        endpoints.MapGet($"api/{SwazyConstants.ServiceModuleApi}/all", async (
                [FromServices] SwazyDbContext db) =>
            {
                Log.Verbose("[ServiceModule - GetAll] Invoked.");

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

                    Log.Debug("[ServiceModule - GetAll] Returned {Count} services.", response.Count);

                    return Results.Ok(response);
                }
                catch (Exception ex)
                {
                    Log.Error("[ServiceModule - GetAll] Error occurred. Exception: {Exception}", ex);
                    return Results.Problem(statusCode: (int)HttpStatusCode.InternalServerError);
                }
            })
            .WithTags(SwazyConstants.ServiceModuleName);

        endpoints.MapPut($"api/{SwazyConstants.ServiceModuleApi}", async (
                [FromServices] SwazyDbContext db,
                [FromBody] UpdateServiceDto updateServiceDto) =>
            {
                Log.Verbose("[ServiceModule - Update] Invoked. {ServiceId}", updateServiceDto.Id);

                try
                {
                    var service = await db.Services.FindAsync(updateServiceDto.Id);

                    if (service == null)
                    {
                        Log.Debug("[ServiceModule - Update] Not found. {ServiceId}", updateServiceDto.Id);
                        return Results.NotFound("Service not found.");
                    }

                    service.Tag = updateServiceDto.Tag;
                    service.BusinessType = updateServiceDto.BusinessType;
                    service.Value = updateServiceDto.Value;

                    await db.SaveChangesAsync();

                    Log.Debug("[ServiceModule - Update] Successfully updated. {ServiceId}", service.Id);

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
                    Log.Error("[ServiceModule - Update] Error occurred. {ServiceId} Exception: {Exception}",
                        updateServiceDto.Id, ex);
                    return Results.Problem(statusCode: (int)HttpStatusCode.InternalServerError);
                }
            })
            .WithTags(SwazyConstants.ServiceModuleName);

        endpoints.MapDelete($"api/{SwazyConstants.ServiceModuleApi}/{{id:guid}}", async (
                [FromServices] SwazyDbContext db,
                [FromRoute] Guid id) =>
            {
                Log.Verbose("[ServiceModule - Delete] Invoked. {ServiceId}", id);

                try
                {
                    var service = await db.Services.FindAsync(id);

                    if (service == null)
                    {
                        Log.Debug("[ServiceModule - Delete] Not found. {ServiceId}", id);
                        return Results.NotFound("Service not found.");
                    }

                    service.IsDeleted = true;
                    service.DeletedAt = DateTimeOffset.UtcNow;

                    await db.SaveChangesAsync();

                    Log.Debug("[ServiceModule - Delete] Successfully soft deleted. {ServiceId}", id);

                    return Results.NoContent();
                }
                catch (Exception ex)
                {
                    Log.Error("[ServiceModule - Delete] Error occurred. {ServiceId} Exception: {Exception}",
                        id, ex);
                    return Results.Problem(statusCode: (int)HttpStatusCode.InternalServerError);
                }
            })
            .WithTags(SwazyConstants.ServiceModuleName);
    }
}