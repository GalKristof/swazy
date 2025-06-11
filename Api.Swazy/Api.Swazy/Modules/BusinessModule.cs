using Api.Swazy.Common;
using Api.Swazy.Models.DTOs.Businesses;
using Api.Swazy.Models.Results;
using Api.Swazy.Services.Businesses;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace Api.Swazy.Modules;

public static class BusinessModule
{
    public static void MapBusinessEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost($"api/{SwazyConstants.BusinessModuleApi}", async (
                [FromServices] IBusinessService businessService,
                [FromBody] CreateBusinessDto createBusinessDto) =>
            {
                var response = await businessService.CreateEntityAsync(createBusinessDto);
                
                return response.Result switch
                {
                    CommonResult.Success => Results.Ok(response.Value),
                    _ => Results.Problem(statusCode: (int)HttpStatusCode.InternalServerError)
                };
            })
            .WithTags(SwazyConstants.BusinessModuleName);

        endpoints.MapGet($"api/{SwazyConstants.BusinessModuleApi}/all", async (
                [FromServices] IBusinessService businessService) =>
            {
                var response = await businessService.GetAllEntitiesAsync();
                
                return response.Result switch
                {
                    CommonResult.Success => Results.Ok(response.Value),
                    _ => Results.Problem(statusCode: (int)HttpStatusCode.InternalServerError)
                };
            })
            .WithTags(SwazyConstants.BusinessModuleName);
        
        endpoints.MapGet($"api/{SwazyConstants.BusinessModuleApi}/{{businessId:guid}}", async (
                [FromServices] IBusinessService businessService,
                [FromRoute] Guid businessId) =>
            {
                var response = await businessService.GetSingleEntityByIdAsync(businessId);

                if (response.Result == CommonResult.Success)
                {
                    return Results.Ok(response.Value);
                }

                return Results.Problem(statusCode: (int)HttpStatusCode.InternalServerError);
            })
            .WithTags(SwazyConstants.BusinessModuleName);
        
        endpoints.MapPut($"api/{SwazyConstants.BusinessModuleApi}", async (
                [FromServices] IBusinessService businessService,
                [FromBody] UpdateBusinessDto updateBusinessDto) =>
            {
                var response = await businessService.UpdateEntityAsync(updateBusinessDto);

                return response.Result switch
                {
                    CommonResult.Success => Results.Ok(response.Value),
                    CommonResult.NotFound => Results.NotFound("Business not found."),
                    _ => Results.Problem(statusCode: (int)HttpStatusCode.InternalServerError)
                };
            })
            .WithTags(SwazyConstants.BusinessModuleName);

        endpoints.MapPut($"api/{SwazyConstants.BusinessModuleApi}/add-employee", async (
                [FromServices] IBusinessService businessService,
                [FromBody] AddEmployeeToBusinessDto addEmployeeDto) =>
            {
                var response = await businessService.AddEmployeeAsync(addEmployeeDto);

                return response.Result switch
                {
                    CommonResult.Success => Results.Ok(response.Value),
                    CommonResult.NotFound => Results.NotFound("Business not found."),
                    CommonResult.RequirementNotFound => Results.NotFound("User not found."),
                    CommonResult.AlreadyIncluded => Results.BadRequest("Employee already included inside business."),
                    _ => Results.Problem(statusCode: (int)HttpStatusCode.InternalServerError)
                };
            })
            .WithTags(SwazyConstants.BusinessModuleName);
        
        endpoints.MapDelete($"api/{SwazyConstants.BusinessModuleApi}/{{id:guid}}", async (
                [FromServices] IBusinessService businessService,
                [FromRoute] Guid id) =>
            {
                var response = await businessService.DeleteEntityAsync(id);
                
                return response.Result switch
                {
                    CommonResult.Success => Results.Ok(response.Value),
                    CommonResult.NotFound => Results.NotFound("Business not found."),
                    _ => Results.Problem(statusCode: (int)HttpStatusCode.InternalServerError)
                };
            })
            .WithTags(SwazyConstants.BusinessModuleName);
    }
}
