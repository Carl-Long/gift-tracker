using System.Linq.Expressions;
using GiftTracker.Application.Features.Recipients.Dtos;
using GiftTracker.Domain.Entities;

namespace GiftTracker.Application.Features.Recipients.Mapping;

internal static class RecipientProjections
{
    public static readonly Expression<Func<Recipient, RecipientDto>> ToRecipientDto = recipient => new RecipientDto(
        recipient.Id,
        recipient.Name,
        recipient.Description,
        recipient.BirthDate,
        recipient.Relationship,
        recipient.IsArchived);
}
