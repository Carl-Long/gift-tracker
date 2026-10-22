using System.ComponentModel.DataAnnotations;
using GiftTracker.Domain.Entities;
using GiftTracker.Domain.Enums;

namespace GiftTracker.Application.Features.Recipients.Dtos;

public sealed class CreateRecipientDto
{
    [Required]
    [StringLength(Recipient.MaxNameLength)]
    public string Name { get; init; } = string.Empty;

    [StringLength(Recipient.MaxDescriptionLength)]
    public string? Description { get; init; }

    public DateOnly? BirthDate { get; init; }

    [EnumDataType(typeof(RecipientRelationship))]
    public RecipientRelationship? Relationship { get; init; }
}
