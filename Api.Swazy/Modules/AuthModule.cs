using System.Net;
using Api.Swazy.Common;
using Api.Swazy.Models.DTOs.Users;
using Api.Swazy.Models.Results;
using Api.Swazy.Services.Auth;
using Microsoft.AspNetCore.Mvc;

namespace Api.Swazy.Modules;

public static class AuthModule
{
    public static void MapAuthEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost($"api/{SwazyConstants.AuthModuleApi}/login", async (
                [FromServices] IAuthService authService,
                [FromBody] LoginUserDto loginUserDto) =>
            {
                var response = await authService.LoginUserAsync(loginUserDto);

                return response.Result switch
                {
                    CommonResult.Success => Results.Ok(response.Value),
                    CommonResult.InvalidCredentials => Results.BadRequest("Invalid credentials."),
                    _ => Results.Problem(statusCode: (int)HttpStatusCode.InternalServerError)
                };
            })
            .WithTags(SwazyConstants.AuthModuleName);

        endpoints.MapPost($"api/{SwazyConstants.AuthModuleApi}/register", async (
                [FromServices] IAuthService authService,
                [FromBody] RegisterUserDto registerUserDto) =>
            {
                var response = await authService.RegisterUserAsync(registerUserDto);

                return response.Result switch
                {
                    CommonResult.Success => Results.Ok(response.Value),
                    CommonResult.UserAlreadyExists => Results.Conflict("User with this email already exists."), // Added UserAlreadyExists
                    CommonResult.InvalidCredentials => Results.BadRequest("Invalid credentials provided for registration."), // Should not happen with register but as a general case
                    _ => Results.Problem(statusCode: (int)HttpStatusCode.InternalServerError)
                };
            })
            .WithTags(SwazyConstants.AuthModuleName);
    }
}