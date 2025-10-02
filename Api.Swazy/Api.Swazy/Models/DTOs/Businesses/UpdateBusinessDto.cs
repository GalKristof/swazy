namespace Api.Swazy.Models.DTOs.Businesses;

/// <summary>
/// DTO for tenant business updates - BusinessType and WebsiteUrl are read-only for tenants
/// </summary>
public record UpdateBusinessDto(
    Guid Id,
    string Name,
    string Address,
    string PhoneNumber,
    string Email,
    string Title,
    string Subtitle,
    string Description,
    string Footer,
    string Theme
    ) : BaseUpdateDto(Id);