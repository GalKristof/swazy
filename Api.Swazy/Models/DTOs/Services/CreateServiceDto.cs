using Api.Swazy.Types;

namespace Api.Swazy.Models.DTOs.Services;

public record CreateServiceDto(
    string Tag,
    BusinessType BusinessType,
    string Value);
