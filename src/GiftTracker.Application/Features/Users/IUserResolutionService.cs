namespace GiftTracker.Application.Features.Users;

public interface IUserResolutionService
{
    Task<Guid> GetOrCreateUserIdAsync(CancellationToken  cancellationToken);
}
