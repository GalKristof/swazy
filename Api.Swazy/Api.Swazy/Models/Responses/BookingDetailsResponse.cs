namespace Api.Swazy.Models.Responses;

public record BookingDetailsResponse(
    Guid Id,
    string ConfirmationCode,
    DateTimeOffset StartTime,
    DateTimeOffset EndTime,
    string ServiceName,
    int ServiceDuration,
    decimal ServicePrice,
    string FirstName,
    string LastName,
    string Email,
    string PhoneNumber,
    string? Notes,
    string? EmployeeName,
    string BusinessName,
    string? BusinessAddress,
    string? BusinessPhone,
    Guid? EmployeeId,
    Guid? BookedByUserId
);