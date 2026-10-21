using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace GiftTracker.Api.Extensions;

public static class AuthenticationExtensions
{
    public static IServiceCollection AddApiAuthentication(this IServiceCollection services, IConfiguration configuration, IHostEnvironment environment)
    {
        var authority = configuration["Authentication:Authority"]
                        ?? throw new InvalidOperationException("Authentication authority is not configured.");

        var audience = configuration["Authentication:Audience"]
                       ?? throw new InvalidOperationException("Authentication audience is not configured.");

        services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.Authority = authority;
                options.Audience = audience;
                options.RequireHttpsMetadata = !environment.IsDevelopment();
                options.MapInboundClaims = false;
            });

        services.AddAuthorization();

        return services;
    }
}
