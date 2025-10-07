using Api.Swazy.Common;
using Api.Swazy.Models.DTOs.Authentication;
using Api.Swazy.Options;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace Api.Swazy.Providers;

public class JwtTokenProvider : IJwtTokenProvider
{
    private readonly JwtOptions jwtOptions;
    private readonly JwtSecurityTokenHandler tokenHandler = new();
    private readonly TokenValidationParameters tokenValidationParameters;
    private readonly SigningCredentials accessTokenSigningCredentials;

    public JwtTokenProvider(IOptions<JwtOptions> options)
    {
        jwtOptions = options.Value;
        var accessTokenKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.AccessSecretKey));

        tokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = accessTokenKey,

            ValidateIssuer = true,
            ValidIssuer = jwtOptions.Issuer,

            ValidateAudience = true,
            ValidAudience = jwtOptions.Audience,

            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromSeconds(SwazyConstants.JwtTokenClockSkewSeconds)
        };

        accessTokenSigningCredentials = new SigningCredentials(
            accessTokenKey,
            SecurityAlgorithms.HmacSha256
        );
    }
    
    public string GenerateAccessToken(TokenDto tokenDto)
    {
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, tokenDto.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, tokenDto.Email),
            new(JwtRegisteredClaimNames.GivenName, tokenDto.FirstName),
            new(JwtRegisteredClaimNames.FamilyName, tokenDto.LastName),
            new("system_role", tokenDto.SystemRole.ToString())
        };

        if (tokenDto.CurrentBusinessId.HasValue)
        {
            claims.Add(new Claim("business_id", tokenDto.CurrentBusinessId.Value.ToString()));
        }

        if (tokenDto.CurrentBusinessRole.HasValue)
        {
            claims.Add(new Claim("business_role", tokenDto.CurrentBusinessRole.Value.ToString()));
        }

        JwtSecurityToken token = new(
            jwtOptions.Issuer,
            jwtOptions.Audience,
            claims,
            null,
            DateTime.UtcNow.AddMinutes(SwazyConstants.JwtAccessTokenLifetimeMinutes),
            accessTokenSigningCredentials
        );

        return tokenHandler.WriteToken(token);
    }

    public string GenerateRefreshToken()
    {
        var randomNumber = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        return Convert.ToBase64String(randomNumber);
    }

    public bool ValidateToken(string token, bool withLifeTime = true)
    {
        try
        {
            tokenValidationParameters.ValidateLifetime = withLifeTime;
            var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out _);
            return principal is { Identity.IsAuthenticated: true };
        }
        catch
        {
            return false;
        }
    }

    public ClaimsPrincipal? GetPrincipalFromToken(string token, bool validateLifetime = false)
    {
        try
        {
            var validationParameters = tokenValidationParameters.Clone();
            validationParameters.ValidateLifetime = validateLifetime;

            var principal = tokenHandler.ValidateToken(token, validationParameters, out _);
            return principal;
        }
        catch
        {
            return null;
        }
    }

    public string? ReadValueFromToken(string token, string valueName)
    {
        var readToken = tokenHandler.ReadToken(token) as JwtSecurityToken;
        return readToken?.Claims.FirstOrDefault(claim => claim.Type == valueName)?.Value;
    }
}
