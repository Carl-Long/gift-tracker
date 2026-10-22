using GiftTracker.Application.Abstractions.Persistence;
using GiftTracker.Application.Common.Exceptions;
using GiftTracker.Application.Features.Recipients.Dtos;
using GiftTracker.Application.Features.Recipients.Mapping;
using GiftTracker.Application.Features.Users;
using GiftTracker.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace GiftTracker.Application.Features.Recipients.Services;

public sealed class RecipientService(IGiftTrackerDbContext dbContext, IUserResolutionService userResolutionService)
    : IRecipientService
{
    public async Task<IReadOnlyList<RecipientDto>> GetRecipientsAsync(
        bool includeArchived = false,
        CancellationToken cancellationToken = default)
    {
        var userId = await userResolutionService.GetOrCreateUserIdAsync(cancellationToken);

        var query = dbContext.Recipients.AsNoTracking().Where(recipient => recipient.UserId == userId);

        if (!includeArchived)
        {
            query = query.Where(recipient => !recipient.IsArchived);
        }

        return await query.OrderBy(recipient => recipient.Name)
            .Select(RecipientProjections.ToRecipientDto)
            .ToListAsync(cancellationToken);
    }

    public async Task<RecipientDto> GetRecipientByIdAsync(
        Guid recipientId,
        CancellationToken cancellationToken = default)
    {
        var userId = await userResolutionService.GetOrCreateUserIdAsync(cancellationToken);

        var recipient = await dbContext.Recipients.AsNoTracking()
            .Where(recipient => recipient.Id == recipientId && recipient.UserId == userId)
            .Select(RecipientProjections.ToRecipientDto)
            .SingleOrDefaultAsync(cancellationToken);

        return recipient ?? throw new NotFoundException("Recipient not found.");
    }

    public async Task<RecipientDto> CreateRecipientAsync(
        CreateRecipientDto createRecipientDto,
        CancellationToken cancellationToken = default)
    {
        var userId = await userResolutionService.GetOrCreateUserIdAsync(cancellationToken);

        var recipient = new Recipient(
            userId,
            createRecipientDto.Name,
            createRecipientDto.Description,
            createRecipientDto.BirthDate,
            createRecipientDto.Relationship);

        dbContext.Recipients.Add(recipient);

        await dbContext.SaveChangesAsync(cancellationToken);

        return RecipientMapper.ToRecipientDto(recipient);
    }

    public async Task<RecipientDto> UpdateRecipientAsync(
        Guid recipientId,
        UpdateRecipientDto updateRecipientDto,
        CancellationToken cancellationToken = default)
    {
        var userId = await userResolutionService.GetOrCreateUserIdAsync(cancellationToken);

        var recipient = await dbContext.Recipients.SingleOrDefaultAsync(
            recipient => recipient.Id == recipientId && recipient.UserId == userId,
            cancellationToken);

        if (recipient is null)
        {
            throw new NotFoundException("Recipient not found.");
        }

        recipient.UpdateDetails(
            updateRecipientDto.Name,
            updateRecipientDto.Description,
            updateRecipientDto.BirthDate,
            updateRecipientDto.Relationship);

        await dbContext.SaveChangesAsync(cancellationToken);

        return RecipientMapper.ToRecipientDto(recipient);
    }

    public async Task SetRecipientArchivedStatusAsync(
        Guid recipientId,
        bool isArchived,
        CancellationToken cancellationToken = default)
    {
        var userId = await userResolutionService.GetOrCreateUserIdAsync(cancellationToken);

        var recipient = await dbContext.Recipients.SingleOrDefaultAsync(
            recipient => recipient.Id == recipientId && recipient.UserId == userId,
            cancellationToken);

        if (recipient is null)
        {
            throw new NotFoundException("Recipient not found.");
        }

        if (isArchived)
        {
            recipient.Archive();
        }
        else
        {
            recipient.Restore();
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    // For now, allow any delete, in future, will not allow to delete if recipient has any occasions/gift history?
    public async Task DeleteRecipientAsync(Guid recipientId, CancellationToken cancellationToken = default)
    {
        var userId = await userResolutionService.GetOrCreateUserIdAsync(cancellationToken);

        var recipient = await dbContext.Recipients.SingleOrDefaultAsync(
            recipient => recipient.Id == recipientId && recipient.UserId == userId,
            cancellationToken);

        if (recipient is null)
        {
            throw new NotFoundException("Recipient not found.");
        }

        dbContext.Recipients.Remove(recipient);

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
