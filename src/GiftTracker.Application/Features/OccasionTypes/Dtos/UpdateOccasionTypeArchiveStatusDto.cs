using System.ComponentModel.DataAnnotations;

namespace GiftTracker.Application.Features.OccasionTypes.Dtos;

public sealed class UpdateOccasionTypeArchiveStatusDto
{
    public required bool IsArchived { get; init; }
}
