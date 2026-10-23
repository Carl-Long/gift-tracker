using GiftTracker.Application.Abstractions.Persistence;
using GiftTracker.Application.Common.Exceptions;
using GiftTracker.Application.Features.Occasions.Dtos;
using GiftTracker.Application.Features.Occasions.Mappings;
using GiftTracker.Application.Features.Users;
using Microsoft.EntityFrameworkCore;

namespace GiftTracker.Application.Features.Occasions.Services;

public sealed class OccasionService(IGiftTrackerDbContext dbContext, IUserResolutionService userResolutionService)
    : IOccasionService
{
    public async Task<IReadOnlyList<OccasionSummaryDto>> GetOccasionsAsync(
        bool includeArchived = false,
        CancellationToken cancellationToken = default)
    {
        var userId = await userResolutionService.GetOrCreateUserIdAsync(cancellationToken);

        var query = dbContext.Occasions.AsNoTracking().Where(occasion => occasion.UserId == userId);

        if (!includeArchived)
        {
            query = query.Where(occasion => !occasion.IsArchived);
        }

        return await query.OrderBy(occasion => occasion.OccasionDate)
            .ThenBy(occasion => occasion.Name)
            .Join(
                dbContext.OccasionTypes.AsNoTracking(),
                occasion => occasion.OccasionTypeId,
                occasionType => occasionType.Id,
                OccasionProjections.ToOccasionSummaryDto)
            .ToListAsync(cancellationToken);
    }

    public async Task<OccasionDto> GetOccasionByIdAsync(Guid occasionId, CancellationToken cancellationToken = default)
    {
        var userId = await userResolutionService.GetOrCreateUserIdAsync(cancellationToken);

        var occasion = await dbContext.Occasions.AsNoTracking()
            .Where(occasion => occasion.Id == occasionId && occasion.UserId == userId)
            .Join(
                dbContext.OccasionTypes.AsNoTracking(),
                occasion => occasion.OccasionTypeId,
                occasionType => occasionType.Id,
                OccasionProjections.ToOccasionDto)
            .SingleOrDefaultAsync(cancellationToken);

        return occasion ?? throw new NotFoundException("Occasion not found.");
    }
}
