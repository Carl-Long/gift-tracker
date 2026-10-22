using Microsoft.AspNetCore.Authorization;
using Microsoft.OpenApi;
using Scalar.AspNetCore;

namespace GiftTracker.Api.Extensions;

public static class ApiDocumentationExtensions
{
    public static IServiceCollection AddApiDocumentation(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddApiVersioningConfiguration()
            .AddOpenApi(options =>
            {
                options.Document.AddDocumentTransformer((document, _, _) =>
                {
                    var authority = configuration["Authentication:Authority"] ??
                                    throw new InvalidOperationException("Authentication authority is not configured.");

                    document.Components ??= new OpenApiComponents();

                    document.Components.SecuritySchemes ??= new Dictionary<string, IOpenApiSecurityScheme>();

                    document.Components.SecuritySchemes["OAuth2"] = new OpenApiSecurityScheme
                    {
                        Type = SecuritySchemeType.OAuth2,
                        Flows = new OpenApiOAuthFlows
                        {
                            AuthorizationCode = new OpenApiOAuthFlow
                            {
                                AuthorizationUrl = new Uri($"{authority}/protocol/openid-connect/auth"),
                                TokenUrl = new Uri($"{authority}/protocol/openid-connect/token"),
                                Scopes = new Dictionary<string, string> { ["openid"] = "OpenID Connect" }
                            }
                        }
                    };

                    return Task.CompletedTask;
                });

                options.Document.AddOperationTransformer((operation, context, _) =>
                {
                    var metadata = context.Description.ActionDescriptor.EndpointMetadata;

                    var requiresAuthorization = metadata.OfType<IAuthorizeData>().Any();

                    var allowsAnonymous = metadata.OfType<IAllowAnonymous>().Any();

                    if (requiresAuthorization && !allowsAnonymous)
                    {
                        operation.Security ??= [];

                        operation.Security.Add(
                            new OpenApiSecurityRequirement
                            {
                                [new OpenApiSecuritySchemeReference("OAuth2", context.Document)] = []
                            });
                    }

                    return Task.CompletedTask;
                });
            });

        return services;
    }

    public static WebApplication MapApiDocumentation(this WebApplication app)
    {
        var clientId = app.Configuration["ApiDocumentation:Scalar:ClientId"] ??
                       throw new InvalidOperationException("Scalar client ID is not configured.");

        var redirectUri = app.Configuration["ApiDocumentation:Scalar:RedirectUri"] ??
                          throw new InvalidOperationException("Scalar redirect URI is not configured.");

        app.MapOpenApi().WithDocumentPerVersion();

        app.MapScalarApiReference(options =>
        {
            var versions = app.DescribeApiVersions();

            for (var i = 0; i < versions.Count; i++)
            {
                var version = versions[i];
                options.AddDocument(version.GroupName, version.GroupName, isDefault: i == versions.Count - 1);
            }

            options.AddPreferredSecuritySchemes("OAuth2")
                .AddAuthorizationCodeFlow(
                    "OAuth2",
                    flow =>
                    {
                        flow.ClientId = clientId;
                        flow.RedirectUri = redirectUri;
                        flow.Pkce = Pkce.Sha256;
                        flow.SelectedScopes = ["openid"];
                    });
        });

        return app;
    }
}
