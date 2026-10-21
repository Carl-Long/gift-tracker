using GiftTracker.Application.Features.Recipients.Dtos;

namespace GiftTracker.Application.Features.Recipients.Services;

public interface IRecipientService
{
    Task<IReadOnlyList<RecipientDto>> GetRecipientsAsync(bool includeArchived = false, CancellationToken cancellationToken = default);
}
