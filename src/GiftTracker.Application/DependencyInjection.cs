using GiftTracker.Application.Features.Users;
using Microsoft.Extensions.DependencyInjection;

namespace GiftTracker.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IUserResolutionService, UserResolutionService>();
        return services;
    }
}
