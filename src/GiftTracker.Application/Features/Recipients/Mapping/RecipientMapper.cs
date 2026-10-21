using GiftTracker.Application.Features.Recipients.Dtos;
using GiftTracker.Domain.Entities;

namespace GiftTracker.Application.Features.Recipients.Mapping;

internal static class RecipientMapper
{
    public static RecipientDto ToRecipientDto(Recipient recipient)
    {
        return new RecipientDto(
            recipient.Id,
            recipient.Name,
            recipient.Description,
            recipient.BirthDate,
            recipient.Relationship,
            recipient.IsArchived);
    }
}
