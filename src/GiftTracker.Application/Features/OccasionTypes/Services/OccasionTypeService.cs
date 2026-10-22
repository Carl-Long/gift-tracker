using GiftTracker.Application.Abstractions.Persistence;
using GiftTracker.Application.Common.Exceptions;
using GiftTracker.Application.Features.OccasionTypes.Dtos;
using GiftTracker.Application.Features.OccasionTypes.Mappings;
using GiftTracker.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace GiftTracker.Application.Features.OccasionTypes.Services;

public sealed class OccasionTypeService(IGiftTrackerDbContext dbContext) : IOccasionTypeService
{
    public async Task<IReadOnlyList<OccasionTypeDto>> GetOccasionTypesAsync(
        bool includeArchived = false,
        CancellationToken cancellationToken = default)
    {
        var query = dbContext.OccasionTypes.AsNoTracking().AsQueryable();

        if (!includeArchived)
        {
            query = query.Where(occasionType => !occasionType.IsArchived);
        }

        return await query.OrderBy(x => x.Name)
            .ThenBy(x => x.Id)
            .Select(x => new OccasionTypeDto(x.Id, x.Name, x.Description, x.IsArchived))
            .ToListAsync(cancellationToken);
    }

    public async Task<OccasionTypeDto> GetOccasionTypeByIdAsync(
        Guid occasionTypeId,
        CancellationToken cancellationToken = default)
    {
        var occasionType = await dbContext.OccasionTypes.AsNoTracking()
            .Where(occasionType => occasionType.Id == occasionTypeId)
            .Select(x => new OccasionTypeDto(x.Id, x.Name, x.Description, x.IsArchived))
            .SingleOrDefaultAsync(cancellationToken);

        return occasionType ?? throw new NotFoundException("Occasion type not found.");
    }

    public async Task<OccasionTypeDto> CreateOccasionTypeAsync(
        CreateOccasionTypeDto createOccasionTypeDto,
        CancellationToken cancellationToken = default)
    {
        var occasionType = new OccasionType(createOccasionTypeDto.Name, createOccasionTypeDto.Description);

        dbContext.OccasionTypes.Add(occasionType);

        await dbContext.SaveChangesAsync(cancellationToken);

        return OccasionTypeMapper.ToOccasionTypeDto(occasionType);
    }


    public async Task<OccasionTypeDto> UpdateOccasionTypeAsync(
        Guid occasionTypeId,
        UpdateOccasionTypeDto updateOccasionTypeDto,
        CancellationToken cancellationToken = default)
    {
        var occasionType = await dbContext.OccasionTypes.Where(occasionType => occasionType.Id == occasionTypeId)
            .SingleOrDefaultAsync(cancellationToken);

        if (occasionType is null)
        {
            throw new NotFoundException("Occasion type not found.");
        }

        occasionType.UpdateDetails(updateOccasionTypeDto.Name, updateOccasionTypeDto.Description);
        await dbContext.SaveChangesAsync(cancellationToken);

        return OccasionTypeMapper.ToOccasionTypeDto(occasionType);
    }

    public async Task SetOccasionTypeArchivedStatusAsync(
        Guid occasionTypeId,
        bool isArchived,
        CancellationToken cancellationToken = default)
    {
        var occasionType = await dbContext.OccasionTypes.SingleOrDefaultAsync(
            occasionType => occasionType.Id == occasionTypeId,
            cancellationToken);

        if (occasionType is null)
        {
            throw new NotFoundException("Occasion type not found.");
        }

        if (isArchived)
        {
            occasionType.Archive();
        }
        else
        {
            occasionType.Restore();
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteOccasionTypeAsync(Guid occasionTypeId, CancellationToken cancellationToken = default)
    {
        var occasionType = await dbContext.OccasionTypes.SingleOrDefaultAsync(
            occasionType => occasionType.Id == occasionTypeId,
            cancellationToken);

        if (occasionType is null)
        {
            throw new NotFoundException("Occasion type not found.");
        }

        dbContext.OccasionTypes.Remove(occasionType);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
