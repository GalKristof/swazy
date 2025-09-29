using Api.Swazy.Providers;

namespace Api.Swazy.Extensions;

public static class SwazyExtensions
{
    public static void InjectDependencies(this IServiceCollection services)
    {
        // Providers
        services.AddSingleton<IHashingProvider, HashingProvider>();
        services.AddSingleton<IJwtTokenProvider, JwtTokenProvider>();
    }
}
