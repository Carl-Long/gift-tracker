namespace GiftTracker.Application.Features.OccasionTypes.Dtos;

public sealed record OccasionTypeDto(Guid Id, string Name, string? Description, bool IsArchived);
