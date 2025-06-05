using Api.Swazy.Common;
using Api.Swazy.Models.DTOs.Bookings;
using Api.Swazy.Models.Results;
using Api.Swazy.Services.Bookings;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace Api.Swazy.Modules;

public static class BookingModule
{
    public static void MapBookingEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost($"api/{SwazyConstants.BookingModuleApi}", async (
                [FromServices] IBookingService bookingService,
                [FromBody] CreateBookingDto createBookingDto) =>
            {
                var response = await bookingService.CreateEntityAsync(createBookingDto);

                return response.Result switch
                {
                    CommonResult.Success => Results.Ok(response.Value),
                    CommonResult.RequirementNotFound => Results.NotFound("Requirement not found."),
                    _ => Results.Problem(statusCode: (int)HttpStatusCode.InternalServerError)
                };
            })
            .WithTags(SwazyConstants.BookingModuleName);

        endpoints.MapGet($"api/{SwazyConstants.BookingModuleApi}/all", async (
                [FromServices] IBookingService bookingService) =>
            {
                var response = await bookingService.GetAllEntitiesAsync();

                if (response.Result == CommonResult.Success)
                {
                    return Results.Ok(response.Value);
                }

                return Results.Problem(statusCode: (int)HttpStatusCode.InternalServerError);
            })
            .WithTags(SwazyConstants.BookingModuleName);

        endpoints.MapGet($"api/{SwazyConstants.BookingModuleApi}/business/{{businessId:guid}}", async (
                [FromServices] IBookingService bookingService,
                [FromRoute] Guid businessId) =>
            {
                var response = await bookingService.GetBookingsByBusinessIdAsync(businessId);

                if (response.Result == CommonResult.Success)
                {
                    return Results.Ok(response.Value);
                }

                return Results.Problem(statusCode: (int)HttpStatusCode.InternalServerError);
            })
            .WithTags(SwazyConstants.BookingModuleName);

        endpoints.MapPut($"api/{SwazyConstants.BookingModuleApi}", async (
                [FromServices] IBookingService bookingService,
                [FromBody] UpdateBookingDto updateBookingDto) =>
            {
                var response = await bookingService.UpdateEntityAsync(updateBookingDto);

                if (response is { Result: CommonResult.Success, Value: not null })
                {
                    return Results.Ok(response.Value);
                }

                if (response.Result == CommonResult.NotFound)
                {
                    return Results.NotFound("Booking not found.");
                }

                return Results.Problem(statusCode: (int)HttpStatusCode.InternalServerError);
            })
            .WithTags(SwazyConstants.BookingModuleName);

        endpoints.MapDelete($"api/{SwazyConstants.BookingModuleApi}/{{id:guid}}", async (
                [FromServices] IBookingService bookingService,
                [FromRoute] Guid id) =>
            {
                var response = await bookingService.DeleteEntityAsync(id);

                if (response is { Result: CommonResult.Success, Value: not null })
                {
                    return Results.Ok(response.Value);
                }

                if (response.Result == CommonResult.NotFound)
                {
                    return Results.NotFound("Booking not found.");
                }

                return Results.Problem(statusCode: (int)HttpStatusCode.InternalServerError);
            })
            .WithTags(SwazyConstants.BookingModuleName);
    }
}
