using GiftTracker.Application.Abstractions.Persistence;
using GiftTracker.Infrastructure.Persistence;
using GiftTracker.Infrastructure.Persistence.Interceptors;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace GiftTracker.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton(TimeProvider.System);

        services.AddScoped<AuditingInterceptor>();

        services.AddDbContext<GiftTrackerDbContext>((serviceProvider, options) =>
        {
            var connectionString = configuration.GetConnectionString("DefaultConnection")
                                   ?? throw new InvalidOperationException("Database connection string 'DefaultConnection' is not configured.");

            options.UseSqlServer(connectionString);
            options.AddInterceptors(serviceProvider.GetRequiredService<AuditingInterceptor>());
        });

        // Register this way because we want both types to resolve to the same scoped DbContext instance.
        // If we registered the interface independently using AddScoped<IGiftTrackerDbContext, GiftTrackerDbContext>(),
        // we could end up with two separately constructed contexts in the same request.
        services.AddScoped<IGiftTrackerDbContext>(serviceProvider => serviceProvider.GetRequiredService<GiftTrackerDbContext>());

        return services;
    }
}
