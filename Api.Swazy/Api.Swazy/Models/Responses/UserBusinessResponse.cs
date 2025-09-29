namespace Api.Swazy.Models.Responses;

public record UserBusinessResponse(
    Guid BusinessId,
    string BusinessName,
    string Role
);