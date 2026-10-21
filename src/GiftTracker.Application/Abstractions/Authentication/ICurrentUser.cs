namespace GiftTracker.Application.Abstractions.Authentication;

public interface ICurrentUser
{
    string IdentityProvider { get; }
    string ExternalSubjectId { get; }
}
