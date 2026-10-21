using GiftTracker.Application.Abstractions.Authentication;

namespace GiftTracker.Api.Authentication;

public sealed class HttpCurrentUser(IHttpContextAccessor httpContextAccessor) : ICurrentUser
{
    public string IdentityProvider => GetRequiredClaim("iss");
    public string ExternalSubjectId => GetRequiredClaim("sub");

    private string GetRequiredClaim(string claimType)
    {
        var principal = httpContextAccessor.HttpContext?.User;

        if (principal?.Identity?.IsAuthenticated != true)
        {
            throw new InvalidOperationException("An authenticated user is required.");
        }

        return principal.FindFirst(claimType)?.Value
               ?? throw new InvalidOperationException($"Required claim '{claimType}' is missing.");
    }
}
