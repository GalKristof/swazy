namespace Api.Swazy.Models.Responses;

public record BusinessServiceResponse(
    Guid Id,
    Guid BusinessId,
    Guid ServiceId,
    decimal Price,
    int Duration,
    DateTimeOffset CreatedAt
);