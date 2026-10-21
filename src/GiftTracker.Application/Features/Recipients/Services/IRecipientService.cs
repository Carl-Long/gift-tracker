using GiftTracker.Application.Features.Recipients.Dtos;

namespace GiftTracker.Application.Features.Recipients.Services;

public interface IRecipientService
{
    Task<IReadOnlyList<RecipientDto>> GetRecipientsAsync(bool includeArchived = false, CancellationToken cancellationToken = default);
    Task<RecipientDto> GetRecipientByIdAsync(Guid recipientId, CancellationToken cancellationToken = default);
    Task<RecipientDto> CreateRecipientAsync(CreateRecipientDto createRecipientDto, CancellationToken cancellationToken = default);
    Task<RecipientDto> UpdateRecipientAsync(Guid recipientId, UpdateRecipientDto dto, CancellationToken cancellationToken = default);
}
