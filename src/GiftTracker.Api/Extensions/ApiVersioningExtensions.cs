using Asp.Versioning;

namespace GiftTracker.Api.Extensions;

public static class ApiVersioningExtensions
{
    public static IApiVersioningBuilder AddApiVersioningConfiguration(this IServiceCollection services)
    {
        return services.AddApiVersioning(options =>
            {
                options.ReportApiVersions = true;
                options.ApiVersionReader = new UrlSegmentApiVersionReader();
            })
            .AddMvc()
            .AddApiExplorer(options =>
            {
                options.GroupNameFormat = "'v'VVV";
                options.SubstituteApiVersionInUrl = true;
            });
    }
}
