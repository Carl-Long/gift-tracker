namespace GiftTracker.Application.Features.Occasions.Dtos;

public sealed record OccasionDto(
    Guid Id,
    Guid OccasionTypeId,
    string OccasionTypeName,
    string Name,
    string? Description,
    DateOnly OccasionDate,
    int? MaxNumberOfRecipients,
    decimal? Budget,
    bool IsArchived);
