using GiftTracker.Application.Features.OccasionTypes.Dtos;
using GiftTracker.Domain.Entities;

namespace GiftTracker.Application.Features.OccasionTypes.Mappings;

public static class OccasionTypeMapper
{
    public static OccasionTypeDto ToOccasionTypeDto(OccasionType occasionType)
    {
        return new OccasionTypeDto(
            occasionType.Id,
            occasionType.Name,
            occasionType.Description,
            occasionType.IsArchived);
    }
}
