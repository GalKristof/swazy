namespace Api.Swazy.Models.DTOs.Bookings;

public record UpdateBookingDto(
    Guid Id,
    DateTimeOffset BookingDate,
    string? Notes,
    string FirstName,
    string LastName,
    string Email,
    string PhoneNumber,
    Guid BusinessServiceId,
    Guid? EmployeeId,
    Guid? BookedByUserId) : BaseUpdateDto(Id);