namespace Api.Swazy.Models.Responses;

public record UserResponse(
    Guid Id,
    string FirstName,
    string LastName,
    string Email,
    string PhoneNumber,
    string SystemRole,
    List<UserBusinessResponse> Businesses,
    DateTimeOffset CreatedAt
);