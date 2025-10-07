using Api.Swazy.Models.DTOs.Authentication;
using System.Security.Claims;

namespace Api.Swazy.Providers;

public interface IJwtTokenProvider
{
    string GenerateAccessToken(TokenDto tokenDto);

    string GenerateRefreshToken();

    bool ValidateToken(string token, bool withLifeTime = true);

    ClaimsPrincipal? GetPrincipalFromToken(string token, bool validateLifetime = false);

    string? ReadValueFromToken(string token, string valueName);
}
