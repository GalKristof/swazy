namespace Api.Swazy.Models.Responses;

public record BookingResponse(
    Guid Id,
    string ConfirmationCode,
    DateTimeOffset BookingDate,
    string? Notes,
    string FirstName,
    string LastName,
    string Email,
    string PhoneNumber,
    Guid BusinessServiceId,
    Guid? EmployeeId,
    Guid? BookedByUserId,
    DateTimeOffset CreatedAt
);