using Api.Swazy.Common;
using Api.Swazy.Models.DTOs.Authentication;
using Api.Swazy.Options;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
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
        Claim[] claims =
        {
            new(JwtRegisteredClaimNames.Sub, tokenDto.Id.ToString()),
            new(JwtRegisteredClaimNames.GivenName, tokenDto.FirstName),
            new(JwtRegisteredClaimNames.FamilyName, tokenDto.LastName)
        };

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

    public bool ValidateToken(string token, bool withLifeTime = true)
    {
        try
        {
            tokenValidationParameters.ValidateLifetime = withLifeTime;
            var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out _);
            return principal is { Identity.IsAuthenticated: true };
        }
        catch (Exception ex)
        {
            return false;
        }
    }

    public string? ReadValueFromToken(string token, string valueName)
    {
        var readToken = tokenHandler.ReadToken(token) as JwtSecurityToken;
        return readToken?.Claims.First(claim => claim.Type == valueName).Value;
    }
}
