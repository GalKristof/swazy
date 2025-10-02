using Api.Swazy.Common;
using Api.Swazy.Models.DTOs.Bookings;
using Api.Swazy.Models.Entities;
using Api.Swazy.Models.Responses;
using Api.Swazy.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Serilog;
using System.Net;

namespace Api.Swazy.Modules;

public static class BookingModule
{
    public static void MapBookingEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost($"api/{SwazyConstants.BookingModuleApi}", async (
                [FromServices] SwazyDbContext db,
                [FromBody] CreateBookingDto createBookingDto) =>
            {
                Log.Verbose("[BookingModule - Create] Invoked.");

                try
                {
                    // Generate unique confirmation code
                    string confirmationCode;
                    do
                    {
                        confirmationCode = ConfirmationCodeGenerator.Generate();
                    } while (await db.Bookings.AnyAsync(b => b.ConfirmationCode == confirmationCode));

                    var booking = new Booking
                    {
                        ConfirmationCode = confirmationCode,
                        BookingDate = createBookingDto.BookingDate,
                        Notes = createBookingDto.Notes,
                        FirstName = createBookingDto.FirstName,
                        LastName = createBookingDto.LastName,
                        Email = createBookingDto.Email,
                        PhoneNumber = createBookingDto.PhoneNumber,
                        BusinessServiceId = createBookingDto.BusinessServiceId,
                        EmployeeId = createBookingDto.EmployeeId,
                        BookedByUserId = createBookingDto.BookedByUserId
                    };

                    db.Bookings.Add(booking);
                    await db.SaveChangesAsync();

                    Log.Debug("[BookingModule - Create] Successfully created. {BookingId}", booking.Id);

                    var response = new BookingResponse(
                        booking.Id,
                        booking.ConfirmationCode,
                        booking.BookingDate,
                        booking.Notes,
                        booking.FirstName,
                        booking.LastName,
                        booking.Email,
                        booking.PhoneNumber,
                        booking.BusinessServiceId,
                        booking.EmployeeId,
                        booking.BookedByUserId,
                        booking.CreatedAt
                    );

                    return Results.Ok(response);
                }
                catch (Exception ex)
                {
                    Log.Error("[BookingModule - Create] Error occurred. Exception: {Exception}", ex);
                    return Results.Problem(statusCode: (int)HttpStatusCode.InternalServerError);
                }
            })
            .WithTags(SwazyConstants.BookingModuleName);

        endpoints.MapGet($"api/{SwazyConstants.BookingModuleApi}/all", async (
                [FromServices] SwazyDbContext db) =>
            {
                Log.Verbose("[BookingModule - GetAll] Invoked.");

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

                    Log.Debug("[BookingModule - GetAll] Returned {Count} bookings.", response.Count);

                    return Results.Ok(response);
                }
                catch (Exception ex)
                {
                    Log.Error("[BookingModule - GetAll] Error occurred. Exception: {Exception}", ex);
                    return Results.Problem(statusCode: (int)HttpStatusCode.InternalServerError);
                }
            })
            .WithTags(SwazyConstants.BookingModuleName);

        endpoints.MapGet($"api/{SwazyConstants.BookingModuleApi}/business/{{businessId:guid}}", async (
                [FromServices] SwazyDbContext db,
                [FromRoute] Guid businessId) =>
            {
                Log.Verbose("[BookingModule - GetByBusiness] Invoked. {BusinessId}", businessId);

                try
                {
                    var bookings = await db.Bookings
                        .Include(b => b.BusinessService)
                            .ThenInclude(bs => bs!.Service)
                        .Include(b => b.BusinessService)
                            .ThenInclude(bs => bs!.Business)
                        .Include(b => b.Employee)
                        .Where(b => b.BusinessService != null && b.BusinessService.BusinessId == businessId)
                        .ToListAsync();

                    var response = bookings.Select(b => new BookingDetailsResponse(
                        b.Id,
                        b.ConfirmationCode,
                        b.BookingDate,
                        b.BookingDate.AddMinutes(b.BusinessService!.Duration),
                        b.BusinessService!.Service!.Value,
                        b.BusinessService.Duration,
                        b.BusinessService.Price,
                        b.FirstName,
                        b.LastName,
                        b.Email,
                        b.PhoneNumber,
                        b.Notes,
                        b.Employee != null ? $"{b.Employee.FirstName} {b.Employee.LastName}" : null,
                        b.BusinessService.Business!.Name,
                        b.BusinessService.Business.Address,
                        b.BusinessService.Business.PhoneNumber,
                        b.EmployeeId,
                        b.BookedByUserId
                    )).ToList();

                    Log.Debug("[BookingModule - GetByBusiness] Returned {Count} bookings. {BusinessId}", 
                        response.Count, businessId);

                    return Results.Ok(response);
                }
                catch (Exception ex)
                {
                    Log.Error("[BookingModule - GetByBusiness] Error occurred. {BusinessId} Exception: {Exception}", 
                        businessId, ex);
                    return Results.Problem(statusCode: (int)HttpStatusCode.InternalServerError);
                }
            })
            .WithTags(SwazyConstants.BookingModuleName);

        endpoints.MapGet($"api/{SwazyConstants.BookingModuleApi}/customer/{{email}}", async (
                [FromServices] SwazyDbContext db,
                [FromRoute] string email) =>
            {
                Log.Verbose("[BookingModule - GetByCustomerEmail] Invoked. {Email}", email);

                try
                {
                    var bookings = await db.Bookings
                        .Include(b => b.BusinessService)
                            .ThenInclude(bs => bs!.Service)
                        .Include(b => b.BusinessService)
                            .ThenInclude(bs => bs!.Business)
                        .Include(b => b.Employee)
                        .Where(b => b.Email == email)
                        .OrderByDescending(b => b.BookingDate)
                        .ToListAsync();

                    var response = bookings.Select(b => new BookingDetailsResponse(
                        b.Id,
                        b.ConfirmationCode,
                        b.BookingDate,
                        b.BookingDate.AddMinutes(b.BusinessService!.Duration),
                        b.BusinessService!.Service!.Value,
                        b.BusinessService.Duration,
                        b.BusinessService.Price,
                        b.FirstName,
                        b.LastName,
                        b.Email,
                        b.PhoneNumber,
                        b.Notes,
                        b.Employee != null ? $"{b.Employee.FirstName} {b.Employee.LastName}" : null,
                        b.BusinessService.Business!.Name,
                        b.BusinessService.Business.Address,
                        b.BusinessService.Business.PhoneNumber,
                        b.EmployeeId,
                        b.BookedByUserId
                    )).ToList();

                    Log.Debug("[BookingModule - GetByCustomerEmail] Returned {Count} bookings. {Email}", 
                        response.Count, email);

                    return Results.Ok(response);
                }
                catch (Exception ex)
                {
                    Log.Error("[BookingModule - GetByCustomerEmail] Error occurred. {Email} Exception: {Exception}", 
                        email, ex);
                    return Results.Problem(statusCode: (int)HttpStatusCode.InternalServerError);
                }
            })
            .WithTags(SwazyConstants.BookingModuleName);

        endpoints.MapGet($"api/{SwazyConstants.BookingModuleApi}/employee/{{userId:guid}}", async (
                [FromServices] SwazyDbContext db,
                [FromRoute] Guid userId) =>
            {
                Log.Verbose("[BookingModule - GetByEmployee] Invoked. {UserId}", userId);

                try
                {
                    var bookings = await db.Bookings
                        .Include(b => b.BusinessService)
                            .ThenInclude(bs => bs!.Service)
                        .Include(b => b.BusinessService)
                            .ThenInclude(bs => bs!.Business)
                        .Include(b => b.Employee)
                        .Where(b => b.EmployeeId == userId)
                        .OrderByDescending(b => b.BookingDate)
                        .ToListAsync();

                    var response = bookings.Select(b => new BookingDetailsResponse(
                        b.Id,
                        b.ConfirmationCode,
                        b.BookingDate,
                        b.BookingDate.AddMinutes(b.BusinessService!.Duration),
                        b.BusinessService!.Service!.Value,
                        b.BusinessService.Duration,
                        b.BusinessService.Price,
                        b.FirstName,
                        b.LastName,
                        b.Email,
                        b.PhoneNumber,
                        b.Notes,
                        b.Employee != null ? $"{b.Employee.FirstName} {b.Employee.LastName}" : null,
                        b.BusinessService.Business!.Name,
                        b.BusinessService.Business.Address,
                        b.BusinessService.Business.PhoneNumber,
                        b.EmployeeId,
                        b.BookedByUserId
                    )).ToList();

                    Log.Debug("[BookingModule - GetByEmployee] Returned {Count} bookings. {UserId}", 
                        response.Count, userId);

                    return Results.Ok(response);
                }
                catch (Exception ex)
                {
                    Log.Error("[BookingModule - GetByEmployee] Error occurred. {UserId} Exception: {Exception}", 
                        userId, ex);
                    return Results.Problem(statusCode: (int)HttpStatusCode.InternalServerError);
                }
            })
            .WithTags(SwazyConstants.BookingModuleName);

        endpoints.MapPut($"api/{SwazyConstants.BookingModuleApi}", async (
                [FromServices] SwazyDbContext db,
                [FromBody] UpdateBookingDto updateBookingDto) =>
            {
                Log.Verbose("[BookingModule - Update] Invoked. {BookingId}", updateBookingDto.Id);

                try
                {
                    var booking = await db.Bookings.FindAsync(updateBookingDto.Id);

                    if (booking == null)
                    {
                        Log.Debug("[BookingModule - Update] Not found. {BookingId}", updateBookingDto.Id);
                        return Results.NotFound("Booking not found.");
                    }

                    booking.BookingDate = updateBookingDto.BookingDate;
                    booking.Notes = updateBookingDto.Notes;
                    booking.FirstName = updateBookingDto.FirstName;
                    booking.LastName = updateBookingDto.LastName;
                    booking.Email = updateBookingDto.Email;
                    booking.PhoneNumber = updateBookingDto.PhoneNumber;
                    booking.BusinessServiceId = updateBookingDto.BusinessServiceId;
                    booking.EmployeeId = updateBookingDto.EmployeeId;
                    booking.BookedByUserId = updateBookingDto.BookedByUserId;

                    await db.SaveChangesAsync();

                    Log.Debug("[BookingModule - Update] Successfully updated. {BookingId}", booking.Id);

                    var response = new BookingResponse(
                        booking.Id,
                        booking.ConfirmationCode,
                        booking.BookingDate,
                        booking.Notes,
                        booking.FirstName,
                        booking.LastName,
                        booking.Email,
                        booking.PhoneNumber,
                        booking.BusinessServiceId,
                        booking.EmployeeId,
                        booking.BookedByUserId,
                        booking.CreatedAt
                    );

                    return Results.Ok(response);
                }
                catch (Exception ex)
                {
                    Log.Error("[BookingModule - Update] Error occurred. {BookingId} Exception: {Exception}", 
                        updateBookingDto.Id, ex);
                    return Results.Problem(statusCode: (int)HttpStatusCode.InternalServerError);
                }
            })
            .WithTags(SwazyConstants.BookingModuleName);

        endpoints.MapDelete($"api/{SwazyConstants.BookingModuleApi}/{{id:guid}}", async (
                [FromServices] SwazyDbContext db,
                [FromRoute] Guid id) =>
            {
                Log.Verbose("[BookingModule - Delete] Invoked. {BookingId}", id);

                try
                {
                    var booking = await db.Bookings.FindAsync(id);

                    if (booking == null)
                    {
                        Log.Debug("[BookingModule - Delete] Not found. {BookingId}", id);
                        return Results.NotFound("Booking not found.");
                    }

                    booking.IsDeleted = true;
                    booking.DeletedAt = DateTimeOffset.UtcNow;

                    await db.SaveChangesAsync();

                    Log.Debug("[BookingModule - Delete] Successfully soft deleted. {BookingId}", id);

                    var response = new BookingResponse(
                        booking.Id,
                        booking.ConfirmationCode,
                        booking.BookingDate,
                        booking.Notes,
                        booking.FirstName,
                        booking.LastName,
                        booking.Email,
                        booking.PhoneNumber,
                        booking.BusinessServiceId,
                        booking.EmployeeId,
                        booking.BookedByUserId,
                        booking.CreatedAt
                    );

                    return Results.Ok(response);
                }
                catch (Exception ex)
                {
                    Log.Error("[BookingModule - Delete] Error occurred. {BookingId} Exception: {Exception}", 
                        id, ex);
                    return Results.Problem(statusCode: (int)HttpStatusCode.InternalServerError);
                }
            })
            .WithTags(SwazyConstants.BookingModuleName);

        endpoints.MapGet($"api/{SwazyConstants.BookingModuleApi}/confirmation/{{code}}", async (
                [FromServices] SwazyDbContext db,
                [FromRoute] string code) =>
            {
                Log.Verbose("[BookingModule - GetByConfirmationCode] Invoked. {Code}", code);

                try
                {
                    var booking = await db.Bookings
                        .Include(b => b.BusinessService)
                            .ThenInclude(bs => bs!.Service)
                        .Include(b => b.BusinessService)
                            .ThenInclude(bs => bs!.Business)
                        .Include(b => b.Employee)
                        .FirstOrDefaultAsync(b => b.ConfirmationCode == code);

                    if (booking == null)
                    {
                        Log.Debug("[BookingModule - GetByConfirmationCode] Not found. {Code}", code);
                        return Results.NotFound("Booking not found.");
                    }

                    var employeeName = booking.Employee != null
                        ? $"{booking.Employee.FirstName} {booking.Employee.LastName}"
                        : null;

                    var response = new BookingDetailsResponse(
                        booking.Id,
                        booking.ConfirmationCode,
                        booking.BookingDate,
                        booking.BookingDate.AddMinutes(booking.BusinessService!.Duration),
                        booking.BusinessService!.Service!.Value,
                        booking.BusinessService.Duration,
                        booking.BusinessService.Price,
                        booking.FirstName,
                        booking.LastName,
                        booking.Email,
                        booking.PhoneNumber,
                        booking.Notes,
                        employeeName,
                        booking.BusinessService.Business!.Name,
                        booking.BusinessService.Business.Address,
                        booking.BusinessService.Business.PhoneNumber,
                        booking.EmployeeId,
                        booking.BookedByUserId
                    );

                    Log.Debug("[BookingModule - GetByConfirmationCode] Successfully returned. {Code}", code);

                    return Results.Ok(response);
                }
                catch (Exception ex)
                {
                    Log.Error("[BookingModule - GetByConfirmationCode] Error occurred. {Code} Exception: {Exception}",
                        code, ex);
                    return Results.Problem(statusCode: (int)HttpStatusCode.InternalServerError);
                }
            })
            .WithTags(SwazyConstants.BookingModuleName);
    }
}