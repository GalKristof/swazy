namespace Api.Swazy.Models.Responses;

public record BusinessEmployeeResponse(
    Guid UserId,
    string FirstName,
    string LastName,
    string Email,
    string Role
);