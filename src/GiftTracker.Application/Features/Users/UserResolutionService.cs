using GiftTracker.Application.Abstractions.Authentication;
using GiftTracker.Application.Abstractions.Persistence;
using GiftTracker.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace GiftTracker.Application.Features.Users;

public sealed class UserResolutionService(ICurrentUser currentUser, IGiftTrackerDbContext dbContext) : IUserResolutionService
{
    public async Task<Guid> GetOrCreateUserIdAsync(CancellationToken cancellationToken = default)
    {
        var identityProvider = currentUser.IdentityProvider;
        var externalSubjectId = currentUser.ExternalSubjectId;

        var existingUserId = await FindUserIdAsync(identityProvider, externalSubjectId, cancellationToken);

        if (existingUserId.HasValue)
        {
            return existingUserId.Value;
        }

        var user = new User(identityProvider, externalSubjectId);

        dbContext.Users.Add(user);

        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
            return user.Id;
        }
        catch (DbUpdateException)
        {
            // The insert may have failed because another request created the same user concurrently.
            dbContext.Users.Remove(user);

            var concurrentUserId = await FindUserIdAsync(identityProvider, externalSubjectId, cancellationToken);

            if (concurrentUserId.HasValue)
            {
                return concurrentUserId.Value;
            }

            throw;
        }
    }

    private Task<Guid?> FindUserIdAsync(string identityProvider, string externalSubjectId, CancellationToken cancellationToken)
    {
        return dbContext.Users
            .AsNoTracking()
            .Where(user => user.IdentityProvider == identityProvider && user.ExternalSubjectId == externalSubjectId)
            .Select(user => (Guid?)user.Id)
            .SingleOrDefaultAsync(cancellationToken);
    }
}
