using Api.Swazy.Common;
using Api.Swazy.Models.DTOs.Users;
using Api.Swazy.Models.Entities;
using Api.Swazy.Models.Results;
using Api.Swazy.Services.Users;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace Api.Swazy.Modules;

public static class UserModule
{
    public static void MapUserEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost($"api/{SwazyConstants.UserModuleApi}", async (
                [FromServices] IUserService userService,
                [FromServices] IMapper mapper,
                [FromBody] CreateUserDto createUserDto) =>
            {
                var response = await userService.CreateEntityAsync(createUserDto);

                var getUserDto = mapper.Map<GetUserDto>(response.Value);
                
                return response.Result switch
                {
                    CommonResult.Success => Results.Ok(getUserDto),
                    CommonResult.RequirementNotFound => Results.NotFound("Requirement not found."),
                    _ => Results.Problem(statusCode: (int)HttpStatusCode.InternalServerError)
                };
            })
            .WithTags(SwazyConstants.UserModuleName);

        endpoints.MapGet($"api/{SwazyConstants.UserModuleApi}/all", async (
                [FromServices] IUserService userService,
                [FromServices] IMapper mapper) =>
            {
                var response = await userService.GetAllEntitiesAsync();

                if (response is not { Result: CommonResult.Success, Value: not null })
                {
                    return Results.Problem(statusCode: (int)HttpStatusCode.InternalServerError);
                }
                
                var userDtos = response.Value.Select(mapper.Map<GetUserDto>);
                
                return Results.Ok(userDtos);

            })
            .WithTags(SwazyConstants.UserModuleName);
        
        endpoints.MapDelete($"api/{SwazyConstants.UserModuleApi}/{{id:guid}}", async (
                [FromServices] IUserService userService,
                [FromServices] IMapper mapper,
                [FromRoute] Guid id) =>
            {
                var response = await userService.DeleteEntityAsync(id);

                if (response is { Result: CommonResult.Success, Value: not null })
                {
                    var returnUser = mapper.Map<User, GetUserDto>(response.Value);
                    return Results.Ok(returnUser);
                }

                if (response.Result == CommonResult.NotFound)
                {
                    return Results.NotFound("User not found.");
                }

                return Results.Problem(statusCode: (int)HttpStatusCode.InternalServerError);
            })
            .WithTags(SwazyConstants.UserModuleName);

        endpoints.MapPut($"api/{SwazyConstants.UserModuleApi}", async (
                [FromServices] IUserService userService,
                [FromServices] IMapper mapper,
                [FromBody] UpdateUserDto updateUserDto) =>
            {
                var response = await userService.UpdateEntityAsync(updateUserDto);
                
                if (response is { Result: CommonResult.Success, Value: not null })
                {
                    var returnUser = mapper.Map<User, GetUserDto>(response.Value);
                    return Results.Ok(returnUser);
                }
                
                if (response.Result == CommonResult.NotFound)
                {
                    return Results.NotFound("User not found.");
                }
                
                return Results.Problem(statusCode: (int)HttpStatusCode.InternalServerError);
            })
            .WithTags(SwazyConstants.UserModuleName);
    }
}
