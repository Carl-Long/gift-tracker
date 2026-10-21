namespace GiftTracker.Application.Features.Recipients.Dtos;

public sealed class UpdateRecipientArchiveStatusDto
{
    public required bool IsArchived { get; init; }
}
