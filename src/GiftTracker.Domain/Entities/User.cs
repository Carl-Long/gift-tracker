using GiftTracker.Domain.Common;

namespace GiftTracker.Domain.Entities;

public class User : BaseEntity
{
    public const int MaxIdentityProviderLength = 500;
    public const int MaxExternalSubjectIdLength = 255;

    public string IdentityProvider { get; private set; } = string.Empty;
    public string ExternalSubjectId { get; private set; } = string.Empty;

    private User() { } // For EF core

    public User(string identityProvider, string externalSubjectId)
    {
        if (string.IsNullOrWhiteSpace(identityProvider))
        {
            throw new ArgumentException("Identity provider is required.", nameof(identityProvider));
        }

        if (string.IsNullOrWhiteSpace(externalSubjectId))
        {
            throw new ArgumentException("External subject ID is required.", nameof(externalSubjectId));
        }

        var trimmedIdentityProvider = identityProvider.Trim();
        var trimmedExternalSubjectId = externalSubjectId.Trim();

        if (trimmedIdentityProvider.Length > MaxIdentityProviderLength)
        {
            throw new ArgumentException($"Identity provider cannot exceed {MaxIdentityProviderLength} characters.", nameof(identityProvider));
        }

        if (trimmedExternalSubjectId.Length > MaxExternalSubjectIdLength)
        {
            throw new ArgumentException($"External subject ID cannot exceed {MaxExternalSubjectIdLength} characters.", nameof(externalSubjectId));
        }

        IdentityProvider = trimmedIdentityProvider;
        ExternalSubjectId = trimmedExternalSubjectId;
    }
}
