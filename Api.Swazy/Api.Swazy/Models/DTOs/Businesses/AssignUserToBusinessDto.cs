using Api.Swazy.Types;

namespace Api.Swazy.Models.DTOs.Businesses;

public record AssignUserToBusinessDto(
    Guid UserId,
    Guid BusinessId,
    BusinessRole Role
);
