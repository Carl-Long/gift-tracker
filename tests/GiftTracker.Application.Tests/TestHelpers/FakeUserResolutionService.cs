using GiftTracker.Application.Features.Users;

namespace GiftTracker.Application.Tests.TestHelpers;

internal sealed class FakeUserResolutionService(Guid userId) : IUserResolutionService
{
    public Task<Guid> GetOrCreateUserIdAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(userId);
    }
}
