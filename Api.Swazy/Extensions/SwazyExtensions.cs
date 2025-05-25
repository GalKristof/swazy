using Api.Swazy.Persistence.Repositories;
using Api.Swazy.Persistence.UoW;
using Api.Swazy.Providers;
using Api.Swazy.Services.Bookings;
using Api.Swazy.Services.Businesses;
using Api.Swazy.Services.Services;
using Api.Swazy.Services.Users;

namespace Api.Swazy.Extensions;

public static class SwazyExtensions
{
    public static void InjectDependencies(this IServiceCollection services)
    {
        // Repositories
        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

        // Unit Of Work
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IUnitOfWorkFactory, UnitOfWorkFactory>();

        // Services
        services.AddScoped<IBusinessService, BusinessService>();
        services.AddScoped<IServiceService, ServiceService>();
        services.AddScoped<IBookingService, BookingService>();
        services.AddScoped<IUserService, UserService>();

        // Providers
        services.AddSingleton<IHashingProvider, HashingProvider>();
        services.AddSingleton<IJwtTokenProvider, JwtTokenProvider>();
    }
}
