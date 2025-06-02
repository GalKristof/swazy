using Api.Swazy.Types;

namespace Api.Swazy.Models.DTOs.Businesses;

public record CreateBusinessDto(
    string Name,
    string Address,
    string PhoneNumber,
    string Email,
    BusinessType BusinessType,
    string WebsiteUrl);
    
