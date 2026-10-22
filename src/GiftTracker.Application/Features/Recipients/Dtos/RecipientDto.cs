using GiftTracker.Domain.Enums;

namespace GiftTracker.Application.Features.Recipients.Dtos;

public sealed record RecipientDto(
    Guid Id,
    string Name,
    string? Description,
    DateOnly? BirthDate,
    RecipientRelationship? Relationship,
    bool IsArchived);
