using GiftTracker.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GiftTracker.Infrastructure.Persistence.Configurations;

public class OccasionTypeConfiguration : IEntityTypeConfiguration<OccasionType>
{
    public void Configure(EntityTypeBuilder<OccasionType> builder)
    {
        builder.ToTable("OccasionTypes");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name).IsRequired().HasMaxLength(OccasionType.MaxNameLength);

        builder.Property(x => x.Description).HasMaxLength(OccasionType.MaxDescriptionLength);
    }
}
