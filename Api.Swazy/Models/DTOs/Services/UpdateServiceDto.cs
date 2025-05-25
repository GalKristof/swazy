using Api.Swazy.Models.DTOs.Translations;
using Api.Swazy.Types;

namespace Api.Swazy.Models.DTOs.Services;

public record UpdateServiceDto(
    Guid Id,
    string Tag,
    BusinessType BusinessType,
    List<TranslationDto> Translations
) : BaseUpdateDto(Id);