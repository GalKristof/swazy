namespace Api.Swazy.Models.DTOs.Translations;

public record CreateTranslationDto(
    string Key,
    string Language,
    string Value
);
