namespace Api.Swazy.Models.Responses;

public record BookingDetailsResponse(
    Guid Id,
    DateTimeOffset StartTime,
    DateTimeOffset EndTime,
    string ServiceName,
    string FirstName,
    string LastName,
    string Email,
    string PhoneNumber,
    string? Notes,
    Guid? EmployeeId,
    Guid? BookedByUserId
);