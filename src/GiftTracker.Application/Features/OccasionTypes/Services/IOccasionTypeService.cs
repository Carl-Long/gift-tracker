using GiftTracker.Application.Features.OccasionTypes.Dtos;

namespace GiftTracker.Application.Features.OccasionTypes.Services;

public interface IOccasionTypeService
{
    Task<IReadOnlyList<OccasionTypeDto>> GetOccasionTypesAsync(
        bool includeArchived = false,
        CancellationToken cancellationToken = default);

    Task<OccasionTypeDto> GetOccasionTypeByIdAsync(Guid occasionTypeId, CancellationToken cancellationToken = default);

    Task<OccasionTypeDto> CreateOccasionTypeAsync(
        CreateOccasionTypeDto createOccasionTypeDto,
        CancellationToken cancellationToken = default);

    Task<OccasionTypeDto> UpdateOccasionTypeAsync(
        Guid occasionTypeId,
        UpdateOccasionTypeDto updateOccasionTypeDto,
        CancellationToken cancellationToken = default);

    Task SetOccasionTypeArchivedStatusAsync(
        Guid occasionTypeId,
        bool isArchived,
        CancellationToken cancellationToken = default);

    Task DeleteOccasionTypeAsync(Guid occasionTypeId, CancellationToken cancellationToken = default);
}
