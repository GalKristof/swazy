using Api.Swazy.Common;
using Api.Swazy.Models.DTOs.Businesses;
using Api.Swazy.Models.DTOs.BusinessEmployees; // Added
using Api.Swazy.Models.Results;
using Api.Swazy.Services.Businesses;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Security.Claims; // Added
using System; // Added
using Microsoft.AspNetCore.Builder; // Added for RequireAuthorization (might be via WebApplicationExtensions)

namespace Api.Swazy.Modules;

public static class BusinessModule
{
    // Helper function to get performingUserId
    private static Guid GetPerformingUserId(HttpContext httpContext)
    {
        var userIdClaim = httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier); // Or your specific claim type for User ID
        if (Guid.TryParse(userIdClaim, out var userId))
        {
            return userId;
        }
        // This case should ideally not be reached if endpoints are protected by [Authorize]
        // and the token always contains the user ID. Consider how to handle this.
        // Throwing an exception or returning a specific error might be appropriate.
        // For now, let it throw if parsing fails, to highlight missing claim.
        throw new InvalidOperationException("User ID claim is missing or invalid.");
    }

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
            .WithTags(SwazyConstants.BusinessModuleName)
            .RequireAuthorization(); // Assuming POST should be authorized

        endpoints.MapPut($"api/{SwazyConstants.BusinessModuleApi}/add-employee", async (
                HttpContext httpContext, // Added HttpContext
                [FromServices] IBusinessService businessService,
                [FromBody] CreateBusinessEmployeeDto addEmployeeDto) => // Changed DTO
            {
                var performingUserId = GetPerformingUserId(httpContext); // Added
                var response = await businessService.AddEmployeeAsync(addEmployeeDto, performingUserId); // Pass performingUserId

                return response.Result switch
                {
                    CommonResult.Success => Results.Created($"api/businesses/{addEmployeeDto.BusinessId}/employees/{response.Value!.UserId}", response.Value),
                    CommonResult.NotFound => Results.NotFound("Business not found."),
                    CommonResult.RequirementNotFound => Results.NotFound("User to add or hiring user not found."),
                    CommonResult.AlreadyIncluded => Results.Conflict("Employee already exists in this business."),
                    _ => Results.Problem(statusCode: (int)HttpStatusCode.InternalServerError)
                };
            })
            .WithTags(SwazyConstants.BusinessModuleName)
            .RequireAuthorization(); // Added RequireAuthorization

        endpoints.MapGet($"api/{SwazyConstants.BusinessModuleApi}/{{businessId:guid}}/employees", async (
                [FromServices] IBusinessService businessService,
                [FromRoute] Guid businessId) =>
            {
                var response = await businessService.GetBusinessEmployeesAsync(businessId);

                return response.Result switch
                {
                    CommonResult.Success => Results.Ok(response.Value),
                    CommonResult.NotFound => Results.NotFound("Business not found."),
                    _ => Results.Problem(statusCode: (int)HttpStatusCode.InternalServerError)
                };
            })
            .WithTags(SwazyConstants.BusinessModuleName)
            .RequireAuthorization();

        endpoints.MapDelete($"api/{SwazyConstants.BusinessModuleApi}/{{businessId:guid}}/employees/{{userId:guid}}", async (
                HttpContext httpContext, // Added HttpContext
                [FromServices] IBusinessService businessService,
                [FromRoute] Guid businessId,
                [FromRoute] Guid userId) =>
            {
                var performingUserId = GetPerformingUserId(httpContext); // Added
                var response = await businessService.RemoveEmployeeAsync(businessId, userId, performingUserId);

                return response.Result switch
                {
                    CommonResult.Success => Results.Ok(response.Value), // Or Results.NoContent()
                    CommonResult.NotFound => Results.NotFound("Business or Employee not found."),
                    _ => Results.Problem(statusCode: (int)HttpStatusCode.InternalServerError)
                };
            })
            .WithTags(SwazyConstants.BusinessModuleName)
            .RequireAuthorization();

        endpoints.MapPut($"api/{SwazyConstants.BusinessModuleApi}/{{businessId:guid}}/employees/{{userId:guid}}/role", async (
                HttpContext httpContext, // Added HttpContext
                [FromServices] IBusinessService businessService,
                [FromRoute] Guid businessId,
                [FromRoute] Guid userId,
                [FromBody] UpdateBusinessEmployeeDto updateDto) =>
            {
                var performingUserId = GetPerformingUserId(httpContext); // Added
                var response = await businessService.UpdateEmployeeRoleAsync(businessId, userId, updateDto, performingUserId);

                return response.Result switch
                {
                    CommonResult.Success => Results.Ok(response.Value),
                    CommonResult.NotFound => Results.NotFound("Business or Employee not found."),
                    _ => Results.Problem(statusCode: (int)HttpStatusCode.InternalServerError)
                };
            })
            .WithTags(SwazyConstants.BusinessModuleName)
            .RequireAuthorization();

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
