using GiftTracker.Application.Abstractions.Persistence;
using GiftTracker.Application.Features.Recipients.Dtos;
using GiftTracker.Application.Features.Recipients.Mapping;
using GiftTracker.Application.Features.Users;
using Microsoft.EntityFrameworkCore;

namespace GiftTracker.Application.Features.Recipients.Services;

public sealed class RecipientService(IGiftTrackerDbContext dbContext, IUserResolutionService userResolutionService)
    : IRecipientService
{
    public async Task<IReadOnlyList<RecipientDto>> GetRecipientsAsync(bool includeArchived = false, CancellationToken cancellationToken = default)
    {
        var userId = await userResolutionService.GetOrCreateUserIdAsync(cancellationToken);

        var query = dbContext.Recipients.AsNoTracking().Where(recipient => recipient.UserId == userId);

        if (!includeArchived)
        {
            query = query.Where(recipient => !recipient.IsArchived);
        }

        return await query.OrderBy(recipient => recipient.Name)
            .Select(RecipientProjections.ToRecipientDto)
            .ToListAsync(cancellationToken);
    }
}
