using Api.Swazy.Models.DTOs.Authentication;

namespace Api.Swazy.Providers;

public interface IJwtTokenProvider
{
    string GenerateAccessToken(TokenDto tokenDto);

    bool ValidateToken(string token, bool withLifeTime = true);

    string? ReadValueFromToken(string token, string valueName);
}
