using Api.Swazy.Models.Entities;
using Api.Swazy.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Api.Swazy.Services;

public interface IAvailabilityCalculationService
{
    Task<List<DateTimeOffset>> CalculateAvailableTimeSlotsAsync(
        Guid employeeId,
        Guid businessId,
        DateTimeOffset date,
        int serviceDurationMinutes);
}

public class AvailabilityCalculationService : IAvailabilityCalculationService
{
    private readonly SwazyDbContext _db;
    private const int TimeSlotIntervalMinutes = 15;

    public AvailabilityCalculationService(SwazyDbContext db)
    {
        _db = db;
    }

    public async Task<List<DateTimeOffset>> CalculateAvailableTimeSlotsAsync(
        Guid employeeId,
        Guid businessId,
        DateTimeOffset date,
        int serviceDurationMinutes)
    {
        // Get employee's schedule
        var schedule = await _db.EmployeeWeeklySchedules
            .Include(s => s.DaySchedules)
            .FirstOrDefaultAsync(s => s.UserId == employeeId && s.BusinessId == businessId);

        if (schedule == null)
            return new List<DateTimeOffset>();

        // Check if the requested date falls within vacation period
        if (schedule.VacationFrom.HasValue && schedule.VacationTo.HasValue)
        {
            var requestedDate = date.UtcDateTime.Date;
            var vacationStart = schedule.VacationFrom.Value.UtcDateTime.Date;
            var vacationEnd = schedule.VacationTo.Value.UtcDateTime.Date;

            if (requestedDate >= vacationStart && requestedDate <= vacationEnd)
                return new List<DateTimeOffset>();
        }

        // Get day schedule for the requested date
        var dayOfWeek = (int)date.DayOfWeek;
        var daySchedule = schedule.DaySchedules
            .FirstOrDefault(ds => ds.DayOfWeek == dayOfWeek);

        if (daySchedule == null || !daySchedule.IsWorkingDay ||
            daySchedule.StartTime == null || daySchedule.EndTime == null)
            return new List<DateTimeOffset>();

        // Get existing bookings for this employee on this date
        // Convert to UTC to ensure PostgreSQL compatibility
        var startOfDay = date.UtcDateTime.Date;
        var endOfDay = startOfDay.AddDays(1);
        var startOfDayUtc = new DateTimeOffset(startOfDay, TimeSpan.Zero);
        var endOfDayUtc = new DateTimeOffset(endOfDay, TimeSpan.Zero);

        var bookings = await _db.Bookings
            .Include(b => b.BusinessService)
            .Where(b => b.EmployeeId == employeeId &&
                       b.BookingDate >= startOfDayUtc &&
                       b.BookingDate < endOfDayUtc)
            .OrderBy(b => b.BookingDate)
            .ToListAsync();

        // Generate all possible time slots
        var availableSlots = new List<DateTimeOffset>();
        var workingStartTime = date.Date.Add(daySchedule.StartTime.Value);
        var workingEndTime = date.Date.Add(daySchedule.EndTime.Value);

        var currentSlot = workingStartTime;

        while (currentSlot < workingEndTime)
        {
            // Check if service would fit before end of working hours
            var serviceEndTime = currentSlot.AddMinutes(serviceDurationMinutes);
            if (serviceEndTime > workingEndTime)
                break;

            // Check if slot conflicts with existing bookings (including buffer time)
            bool hasConflict = false;
            foreach (var booking in bookings)
            {
                var bookingStart = booking.BookingDate.AddMinutes(-schedule.BufferTimeMinutes);
                var bookingEnd = booking.BookingDate
                    .AddMinutes(booking.BusinessService.Duration)
                    .AddMinutes(schedule.BufferTimeMinutes);

                // Check if current slot overlaps with booking + buffer
                if (currentSlot < bookingEnd && serviceEndTime > bookingStart)
                {
                    hasConflict = true;
                    break;
                }
            }

            if (!hasConflict)
            {
                availableSlots.Add(currentSlot);
            }

            currentSlot = currentSlot.AddMinutes(TimeSlotIntervalMinutes);
        }

        return availableSlots;
    }
}
