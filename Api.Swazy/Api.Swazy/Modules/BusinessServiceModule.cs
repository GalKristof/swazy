using Api.Swazy.Common;
using Api.Swazy.Models.DTOs.BusinessServices;
using Api.Swazy.Models.Results;
using Api.Swazy.Services.BusinessServices;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace Api.Swazy.Modules
{
    public static class BusinessServiceModule
    {
        public static void MapBusinessServiceEndpoints(this IEndpointRouteBuilder endpoints)
        {
            endpoints.MapPost($"api/{SwazyConstants.BusinessServiceModuleApi}", async (
                    [FromServices] IBusinessServiceService businessServiceService,
                    [FromBody] CreateBusinessServiceDto createDto) =>
                {
                    var response = await businessServiceService.CreateEntityAsync(createDto);

                    return response.Result switch
                    {
                        CommonResult.Success => Results.Ok(response.Value),
                        _ => Results.Problem(statusCode: (int)HttpStatusCode.InternalServerError)
                    };
                })
                .WithTags(SwazyConstants.BusinessServiceModuleName);

            endpoints.MapGet($"api/{SwazyConstants.BusinessServiceModuleApi}/business/{{businessId:guid}}", async (
                    [FromServices] IBusinessServiceService businessServiceService,
                    [FromRoute] Guid businessId) =>
                {
                    var response = await businessServiceService.GetBusinessServicesByBusinessIdAsync(businessId);

                    return response.Result switch
                    {
                        CommonResult.Success => Results.Ok(response.Value),
                        CommonResult.NotFound => Results.NotFound("No services found for this business."),
                        _ => Results.Problem(statusCode: (int)HttpStatusCode.InternalServerError, title: "An error occurred while fetching business services.")
                    };
                })
                .WithTags(SwazyConstants.BusinessServiceModuleName);

            endpoints.MapGet($"api/{SwazyConstants.BusinessServiceModuleApi}/all", async (
                    [FromServices] IBusinessServiceService businessServiceService) =>
                {
                    var response = await businessServiceService.GetAllEntitiesAsync();

                    return response.Result switch
                    {
                        CommonResult.Success => Results.Ok(response.Value),
                        _ => Results.Problem(statusCode: (int)HttpStatusCode.InternalServerError)
                    };
                })
                .WithTags(SwazyConstants.BusinessServiceModuleName);

            endpoints.MapGet($"api/{SwazyConstants.BusinessServiceModuleApi}/{{id:guid}}", async (
                    [FromServices] IBusinessServiceService businessServiceService,
                    [FromRoute] Guid id) =>
                {
                    var response = await businessServiceService.GetSingleEntityByIdAsync(id);

                    return response.Result switch
                    {
                        CommonResult.Success => Results.Ok(response.Value),
                        CommonResult.NotFound => Results.NotFound("BusinessService not found."),
                        _ => Results.Problem(statusCode: (int)HttpStatusCode.InternalServerError)
                    };
                })
                .WithTags(SwazyConstants.BusinessServiceModuleName);

            endpoints.MapPut($"api/{SwazyConstants.BusinessServiceModuleApi}", async (
                    [FromServices] IBusinessServiceService businessServiceService,
                    [FromBody] UpdateBusinessServiceDto updateDto) =>
                {
                    var response = await businessServiceService.UpdateEntityAsync(updateDto);

                    return response.Result switch
                    {
                        CommonResult.Success => Results.Ok(response.Value),
                        CommonResult.NotFound => Results.NotFound("BusinessService not found."),
                        _ => Results.Problem(statusCode: (int)HttpStatusCode.InternalServerError)
                    };
                })
                .WithTags(SwazyConstants.BusinessServiceModuleName);

            endpoints.MapDelete($"api/{SwazyConstants.BusinessServiceModuleApi}/{{id:guid}}", async (
                    [FromServices] IBusinessServiceService businessServiceService,
                    [FromRoute] Guid id) =>
                {
                    var response = await businessServiceService.DeleteEntityAsync(id);

                    return response.Result switch
                    {
                        CommonResult.Success => Results.Ok(response.Value),
                        CommonResult.NotFound => Results.NotFound("BusinessService not found."),
                        _ => Results.Problem(statusCode: (int)HttpStatusCode.InternalServerError)
                    };
                })
                .WithTags(SwazyConstants.BusinessServiceModuleName);
        }
    }
}
