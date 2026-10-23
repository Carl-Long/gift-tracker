using GiftTracker.Application.Features.Occasions.Dtos;

namespace GiftTracker.Application.Features.Occasions.Services;

public interface IOccasionService
{
    Task<IReadOnlyList<OccasionSummaryDto>> GetOccasionsAsync(
        bool includeArchived = false,
        CancellationToken cancellationToken = default);

    Task<OccasionDto> GetOccasionByIdAsync(Guid occasionId, CancellationToken cancellationToken);
}
