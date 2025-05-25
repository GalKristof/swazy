using Api.Swazy.Common;
using Api.Swazy.Models.DTOs.Services;
using Api.Swazy.Models.Results;
using Api.Swazy.Services.Services;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace Api.Swazy.Modules;

public static class ServiceModule
{
    public static void MapServiceEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost($"api/{SwazyConstants.ServiceModuleApi}", async (
                [FromServices] IServiceService serviceService,
                [FromServices] IMapper mapper,
                [FromBody] CreateServiceDto createServiceDto) =>
            {
                var response = await serviceService.CreateEntityAsync(createServiceDto);
                
                if (response.Result is not CommonResult.Success || response.Value is null)
                {
                    return Results.Problem(statusCode: (int)HttpStatusCode.InternalServerError);
                }

                var getServiceDto = new GetServiceDto(response.Value.Id, response.Value.Tag, response.Value.BusinessType);

                return Results.Ok(getServiceDto);
            })
            .WithTags(SwazyConstants.ServiceModuleName);

        endpoints.MapGet($"api/{SwazyConstants.ServiceModuleApi}/all", async (
                [FromServices] IServiceService serviceService) =>
            {
                var response = await serviceService.GetAllEntitiesAsync();
                
                if (response.Result != CommonResult.Success || response.Value is null)
                {
                    return Results.Problem(statusCode: (int)HttpStatusCode.InternalServerError);
                }

                return Results.Ok(response.Value);
            })
            .WithTags(SwazyConstants.ServiceModuleName);
        
        endpoints.MapPut($"api/{SwazyConstants.ServiceModuleApi}", async (
                [FromServices] IServiceService serviceService,
                [FromBody] UpdateServiceDto updateServiceDto) =>
            {
                var response = await serviceService.UpdateEntityAsync(updateServiceDto);

                if (response is { Result: CommonResult.Success, Value: not null })
                {
                    return Results.Ok(response.Value);
                }

                if (response.Result == CommonResult.NotFound)
                {
                    return Results.NotFound("Service not found.");
                }

                return Results.Problem(statusCode: (int)HttpStatusCode.InternalServerError);
            })
            .WithTags(SwazyConstants.ServiceModuleName);
        
        endpoints.MapDelete($"api/{SwazyConstants.ServiceModuleApi}/{{id:guid}}", async (
                [FromServices] IServiceService serviceService,
                [FromRoute] Guid id) =>
            {
                var response = await serviceService.DeleteEntityAsync(id);

                if (response is { Result: CommonResult.Success, Value: not null })
                {
                    return Results.Ok(response.Value);
                }

                if (response.Result == CommonResult.NotFound)
                {
                    return Results.NotFound("Service not found.");
                }

                return Results.Problem(statusCode: (int)HttpStatusCode.InternalServerError);
            })
            .WithTags(SwazyConstants.ServiceModuleName);
    }
}
