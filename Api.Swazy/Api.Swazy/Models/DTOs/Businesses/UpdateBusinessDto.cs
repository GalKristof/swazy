using Api.Swazy.Types;

namespace Api.Swazy.Models.DTOs.Businesses;

public record UpdateBusinessDto(
    Guid Id,
    string Name,
    string Address,
    string PhoneNumber,
    string Email,
    BusinessType BusinessType,
    string WebsiteUrl
    ) : BaseUpdateDto(Id);