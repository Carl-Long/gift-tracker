namespace GiftTracker.Domain.Models;

public sealed record OccasionDetails(
    Guid OccasionTypeId,
    string Name,
    string? Description,
    int? MaxNumberOfRecipients,
    decimal? Budget,
    DateOnly OccasionDate);
