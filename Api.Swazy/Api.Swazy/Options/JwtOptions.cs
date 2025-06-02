namespace Api.Swazy.Options;

public class JwtOptions
{
    public string Issuer { get; set; } = string.Empty;

    public string Audience { get; set; } = string.Empty;

    public string AccessSecretKey { get; set; } = string.Empty;
}
