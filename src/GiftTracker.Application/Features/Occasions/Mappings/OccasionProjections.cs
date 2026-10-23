using System.Linq.Expressions;
using GiftTracker.Application.Features.Occasions.Dtos;
using GiftTracker.Domain.Entities;

namespace GiftTracker.Application.Features.Occasions.Mappings;

internal static class OccasionProjections
{
    public static readonly Expression<Func<Occasion, OccasionType, OccasionSummaryDto>> ToOccasionSummaryDto =
        (occasion, occasionType) => new OccasionSummaryDto(
            occasion.Id,
            occasion.OccasionTypeId,
            occasionType.Name,
            occasion.Name,
            occasion.OccasionDate,
            occasion.Budget,
            occasion.IsArchived);

    public static readonly Expression<Func<Occasion, OccasionType, OccasionDto>> ToOccasionDto =
        (occasion, occasionType) => new OccasionDto(
            occasion.Id,
            occasion.OccasionTypeId,
            occasionType.Name,
            occasion.Name,
            occasion.Description,
            occasion.OccasionDate,
            occasion.MaxNumberOfRecipients,
            occasion.Budget,
            occasion.IsArchived);
}
