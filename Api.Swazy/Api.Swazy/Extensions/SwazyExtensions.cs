using Api.Swazy.Providers;
using Api.Swazy.Services;

namespace Api.Swazy.Extensions;

public static class SwazyExtensions
{
    public static void InjectDependencies(this IServiceCollection services)
    {
        // Providers
        services.AddSingleton<IHashingProvider, HashingProvider>();
        services.AddSingleton<IJwtTokenProvider, JwtTokenProvider>();

        // Services
        services.AddScoped<IAvailabilityCalculationService, AvailabilityCalculationService>();
        services.AddScoped<IAuthorizationService, AuthorizationService>();
    }
}
