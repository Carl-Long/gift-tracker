namespace GiftTracker.Application.Features.Occasions.Dtos;

public sealed record OccasionSummaryDto(
    Guid Id,
    Guid OccasionTypeId,
    string OccasionTypeName,
    string Name,
    DateOnly OccasionDate,
    decimal? Budget,
    bool IsArchived);
