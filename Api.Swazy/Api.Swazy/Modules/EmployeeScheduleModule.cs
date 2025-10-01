using Api.Swazy.Common;
using Api.Swazy.Models.DTOs.EmployeeSchedule;
using Api.Swazy.Models.Entities;
using Api.Swazy.Models.Responses;
using Api.Swazy.Persistence;
using Api.Swazy.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Serilog;
using System.Net;

namespace Api.Swazy.Modules;

public static class EmployeeScheduleModule
{
    public static void MapEmployeeScheduleEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost($"api/{SwazyConstants.EmployeeScheduleModuleApi}", async (
                [FromServices] SwazyDbContext db,
                [FromBody] CreateEmployeeScheduleDto dto) =>
            {
                Log.Verbose("[EmployeeScheduleModule - Create] Invoked.");

                try
                {
                    // Check if schedule already exists
                    var existing = await db.EmployeeWeeklySchedules
                        .FirstOrDefaultAsync(s => s.UserId == dto.UserId && s.BusinessId == dto.BusinessId);

                    if (existing != null)
                    {
                        Log.Debug("[EmployeeScheduleModule - Create] Schedule already exists. {UserId} {BusinessId}",
                            dto.UserId, dto.BusinessId);
                        return Results.Conflict("Schedule already exists for this employee and business.");
                    }

                    var schedule = new EmployeeWeeklySchedule
                    {
                        UserId = dto.UserId,
                        BusinessId = dto.BusinessId,
                        BufferTimeMinutes = dto.BufferTimeMinutes,
                        IsOnVacation = dto.IsOnVacation,
                        DaySchedules = dto.DaySchedules.Select(ds => new EmployeeDaySchedule
                        {
                            DayOfWeek = ds.DayOfWeek,
                            IsWorkingDay = ds.IsWorkingDay,
                            StartTime = ds.StartTime,
                            EndTime = ds.EndTime
                        }).ToList()
                    };

                    db.EmployeeWeeklySchedules.Add(schedule);
                    await db.SaveChangesAsync();

                    Log.Debug("[EmployeeScheduleModule - Create] Successfully created. {ScheduleId}", schedule.Id);

                    var response = new EmployeeScheduleResponse(
                        schedule.Id,
                        schedule.UserId,
                        schedule.BusinessId,
                        schedule.BufferTimeMinutes,
                        schedule.IsOnVacation,
                        schedule.DaySchedules.Select(ds => new DayScheduleDto(
                            ds.DayOfWeek,
                            ds.IsWorkingDay,
                            ds.StartTime,
                            ds.EndTime
                        )).ToList(),
                        schedule.CreatedAt
                    );

                    return Results.Ok(response);
                }
                catch (Exception ex)
                {
                    Log.Error("[EmployeeScheduleModule - Create] Error occurred. Exception: {Exception}", ex);
                    return Results.Problem(statusCode: (int)HttpStatusCode.InternalServerError);
                }
            })
            .WithTags(SwazyConstants.EmployeeScheduleModuleName);

        endpoints.MapGet($"api/{SwazyConstants.EmployeeScheduleModuleApi}/business/{{businessId:guid}}", async (
                [FromServices] SwazyDbContext db,
                [FromRoute] Guid businessId) =>
            {
                Log.Verbose("[EmployeeScheduleModule - GetByBusiness] Invoked. {BusinessId}", businessId);

                try
                {
                    var schedules = await db.EmployeeWeeklySchedules
                        .Include(s => s.DaySchedules)
                        .Include(s => s.User)
                        .Where(s => s.BusinessId == businessId)
                        .ToListAsync();

                    var response = schedules.Select(s => new EmployeeScheduleResponse(
                        s.Id,
                        s.UserId,
                        s.BusinessId,
                        s.BufferTimeMinutes,
                        s.IsOnVacation,
                        s.DaySchedules.Select(ds => new DayScheduleDto(
                            ds.DayOfWeek,
                            ds.IsWorkingDay,
                            ds.StartTime,
                            ds.EndTime
                        )).ToList(),
                        s.CreatedAt
                    )).ToList();

                    Log.Debug("[EmployeeScheduleModule - GetByBusiness] Returned {Count} schedules. {BusinessId}",
                        schedules.Count, businessId);

                    return Results.Ok(response);
                }
                catch (Exception ex)
                {
                    Log.Error("[EmployeeScheduleModule - GetByBusiness] Error occurred. {BusinessId} Exception: {Exception}",
                        businessId, ex);
                    return Results.Problem(statusCode: (int)HttpStatusCode.InternalServerError);
                }
            })
            .WithTags(SwazyConstants.EmployeeScheduleModuleName);

        endpoints.MapGet($"api/{SwazyConstants.EmployeeScheduleModuleApi}/employee/{{userId:guid}}/business/{{businessId:guid}}", async (
                [FromServices] SwazyDbContext db,
                [FromRoute] Guid userId,
                [FromRoute] Guid businessId) =>
            {
                Log.Verbose("[EmployeeScheduleModule - GetByEmployee] Invoked. {UserId} {BusinessId}", userId, businessId);

                try
                {
                    var schedule = await db.EmployeeWeeklySchedules
                        .Include(s => s.DaySchedules)
                        .FirstOrDefaultAsync(s => s.UserId == userId && s.BusinessId == businessId);

                    if (schedule == null)
                    {
                        Log.Debug("[EmployeeScheduleModule - GetByEmployee] Not found. {UserId} {BusinessId}",
                            userId, businessId);
                        return Results.NotFound("Schedule not found.");
                    }

                    var response = new EmployeeScheduleResponse(
                        schedule.Id,
                        schedule.UserId,
                        schedule.BusinessId,
                        schedule.BufferTimeMinutes,
                        schedule.IsOnVacation,
                        schedule.DaySchedules.Select(ds => new DayScheduleDto(
                            ds.DayOfWeek,
                            ds.IsWorkingDay,
                            ds.StartTime,
                            ds.EndTime
                        )).ToList(),
                        schedule.CreatedAt
                    );

                    Log.Debug("[EmployeeScheduleModule - GetByEmployee] Found schedule. {ScheduleId}", schedule.Id);

                    return Results.Ok(response);
                }
                catch (Exception ex)
                {
                    Log.Error("[EmployeeScheduleModule - GetByEmployee] Error occurred. {UserId} {BusinessId} Exception: {Exception}",
                        userId, businessId, ex);
                    return Results.Problem(statusCode: (int)HttpStatusCode.InternalServerError);
                }
            })
            .WithTags(SwazyConstants.EmployeeScheduleModuleName);

        endpoints.MapPut($"api/{SwazyConstants.EmployeeScheduleModuleApi}", async (
                [FromServices] SwazyDbContext db,
                [FromBody] UpdateEmployeeScheduleDto dto) =>
            {
                Log.Verbose("[EmployeeScheduleModule - Update] Invoked. {ScheduleId}", dto.Id);

                try
                {
                    var schedule = await db.EmployeeWeeklySchedules
                        .Include(s => s.DaySchedules)
                        .FirstOrDefaultAsync(s => s.Id == dto.Id);

                    if (schedule == null)
                    {
                        Log.Debug("[EmployeeScheduleModule - Update] Not found. {ScheduleId}", dto.Id);
                        return Results.NotFound("Schedule not found.");
                    }

                    schedule.BufferTimeMinutes = dto.BufferTimeMinutes;
                    schedule.IsOnVacation = dto.IsOnVacation;

                    // Update existing day schedules
                    foreach (var dtoDay in dto.DaySchedules)
                    {
                        var existingDay = schedule.DaySchedules
                            .FirstOrDefault(d => d.DayOfWeek == dtoDay.DayOfWeek);

                        if (existingDay != null)
                        {
                            // Update existing
                            existingDay.IsWorkingDay = dtoDay.IsWorkingDay;
                            existingDay.StartTime = dtoDay.StartTime;
                            existingDay.EndTime = dtoDay.EndTime;
                        }
                        else
                        {
                            // Add new
                            schedule.DaySchedules.Add(new EmployeeDaySchedule
                            {
                                EmployeeWeeklyScheduleId = schedule.Id,
                                DayOfWeek = dtoDay.DayOfWeek,
                                IsWorkingDay = dtoDay.IsWorkingDay,
                                StartTime = dtoDay.StartTime,
                                EndTime = dtoDay.EndTime
                            });
                        }
                    }

                    // Remove day schedules that are not in the DTO
                    var daysToRemove = schedule.DaySchedules
                        .Where(d => !dto.DaySchedules.Any(dtoDay => dtoDay.DayOfWeek == d.DayOfWeek))
                        .ToList();

                    foreach (var dayToRemove in daysToRemove)
                    {
                        dayToRemove.IsDeleted = true;
                        dayToRemove.DeletedAt = DateTimeOffset.UtcNow;
                    }

                    await db.SaveChangesAsync();

                    Log.Debug("[EmployeeScheduleModule - Update] Successfully updated. {ScheduleId}", schedule.Id);

                    var response = new EmployeeScheduleResponse(
                        schedule.Id,
                        schedule.UserId,
                        schedule.BusinessId,
                        schedule.BufferTimeMinutes,
                        schedule.IsOnVacation,
                        schedule.DaySchedules.Select(ds => new DayScheduleDto(
                            ds.DayOfWeek,
                            ds.IsWorkingDay,
                            ds.StartTime,
                            ds.EndTime
                        )).ToList(),
                        schedule.CreatedAt
                    );

                    return Results.Ok(response);
                }
                catch (Exception ex)
                {
                    Log.Error("[EmployeeScheduleModule - Update] Error occurred. {ScheduleId} Exception: {Exception}",
                        dto.Id, ex);
                    return Results.Problem(statusCode: (int)HttpStatusCode.InternalServerError);
                }
            })
            .WithTags(SwazyConstants.EmployeeScheduleModuleName);

        endpoints.MapDelete($"api/{SwazyConstants.EmployeeScheduleModuleApi}/{{id:guid}}", async (
                [FromServices] SwazyDbContext db,
                [FromRoute] Guid id) =>
            {
                Log.Verbose("[EmployeeScheduleModule - Delete] Invoked. {ScheduleId}", id);

                try
                {
                    var schedule = await db.EmployeeWeeklySchedules.FindAsync(id);

                    if (schedule == null)
                    {
                        Log.Debug("[EmployeeScheduleModule - Delete] Not found. {ScheduleId}", id);
                        return Results.NotFound("Schedule not found.");
                    }

                    schedule.IsDeleted = true;
                    schedule.DeletedAt = DateTimeOffset.UtcNow;

                    await db.SaveChangesAsync();

                    Log.Debug("[EmployeeScheduleModule - Delete] Successfully soft deleted. {ScheduleId}", id);

                    return Results.Ok();
                }
                catch (Exception ex)
                {
                    Log.Error("[EmployeeScheduleModule - Delete] Error occurred. {ScheduleId} Exception: {Exception}",
                        id, ex);
                    return Results.Problem(statusCode: (int)HttpStatusCode.InternalServerError);
                }
            })
            .WithTags(SwazyConstants.EmployeeScheduleModuleName);

        endpoints.MapGet($"api/{SwazyConstants.EmployeeScheduleModuleApi}/available-slots", async (
                [FromServices] SwazyDbContext db,
                [FromServices] IAvailabilityCalculationService availabilityService,
                [FromQuery] Guid employeeId,
                [FromQuery] Guid businessId,
                [FromQuery] DateTimeOffset date,
                [FromQuery] int serviceDurationMinutes) =>
            {
                Log.Verbose("[EmployeeScheduleModule - GetAvailableSlots] Invoked. {EmployeeId} {Date}",
                    employeeId, date);

                try
                {
                    var availableSlots = await availabilityService.CalculateAvailableTimeSlotsAsync(
                        employeeId,
                        businessId,
                        date,
                        serviceDurationMinutes
                    );

                    var response = new AvailableTimeSlotsResponse(date, availableSlots);

                    Log.Debug("[EmployeeScheduleModule - GetAvailableSlots] Returned {Count} slots. {EmployeeId} {Date}",
                        availableSlots.Count, employeeId, date);

                    return Results.Ok(response);
                }
                catch (Exception ex)
                {
                    Log.Error("[EmployeeScheduleModule - GetAvailableSlots] Error occurred. {EmployeeId} {Date} Exception: {Exception}",
                        employeeId, date, ex);
                    return Results.Problem(statusCode: (int)HttpStatusCode.InternalServerError);
                }
            })
            .WithTags(SwazyConstants.EmployeeScheduleModuleName);
    }
}
