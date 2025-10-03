using Api.Swazy.Types;

namespace Api.Swazy.Models.DTOs.Businesses;

/// <summary>
/// DTO for admin business updates - includes all fields including BusinessType and WebsiteUrl
/// </summary>
public record AdminUpdateBusinessDto(
    Guid Id,
    string Name,
    string Address,
    string PhoneNumber,
    string Email,
    string Title,
    string Subtitle,
    string Description,
    string Footer,
    string Theme,
    BusinessType BusinessType,
    string WebsiteUrl
    ) : BaseUpdateDto(Id);
