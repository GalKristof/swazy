namespace Api.Swazy.Models.Responses;

public record ServiceResponse(
    Guid Id,
    string Tag,
    string BusinessType,
    string Value,
    DateTimeOffset CreatedAt
);