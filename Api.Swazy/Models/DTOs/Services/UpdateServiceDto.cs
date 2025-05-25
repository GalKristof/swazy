using Api.Swazy.Types;

namespace Api.Swazy.Models.DTOs.Services;

public record UpdateServiceDto(
    Guid Id,
    string Tag,
    BusinessType BusinessType,
    string Value
) : BaseUpdateDto(Id);