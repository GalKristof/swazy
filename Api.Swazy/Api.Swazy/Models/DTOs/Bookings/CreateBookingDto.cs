namespace Api.Swazy.Models.DTOs.Bookings;

public record CreateBookingDto(
    DateTimeOffset BookingDate,
    string? Notes,
    string FirstName,
    string LastName,
    string Email,
    string PhoneNumber,
    Guid BusinessServiceId,
    Guid? EmployeeId,
    Guid? BookedByUserId);
