using System.ComponentModel.DataAnnotations;
using GiftTracker.Domain.Entities;

namespace GiftTracker.Application.Features.OccasionTypes.Dtos;

public sealed class CreateOccasionTypeDto
{
    [Required]
    [StringLength(OccasionType.MaxNameLength)]
    public string Name { get; init; } = string.Empty;

    [StringLength(OccasionType.MaxDescriptionLength)]
    public string? Description { get; init; }
}
