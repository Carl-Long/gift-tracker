using GiftTracker.Application.Features.Occasions.Services;
using GiftTracker.Application.Features.OccasionTypes.Services;
using GiftTracker.Application.Features.Recipients.Services;
using GiftTracker.Application.Features.Users;
using Microsoft.Extensions.DependencyInjection;

namespace GiftTracker.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IUserResolutionService, UserResolutionService>();
        services.AddScoped<IRecipientService, RecipientService>();
        services.AddScoped<IOccasionService, OccasionService>();
        services.AddScoped<IOccasionTypeService, OccasionTypeService>();

        return services;
    }
}
