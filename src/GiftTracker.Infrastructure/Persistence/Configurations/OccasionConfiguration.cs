using GiftTracker.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GiftTracker.Infrastructure.Persistence.Configurations;

public class OccasionConfiguration : IEntityTypeConfiguration<Occasion>
{
    public void Configure(EntityTypeBuilder<Occasion> builder)
    {
        builder.ToTable("Occasions");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name).IsRequired().HasMaxLength(Occasion.MaxNameLength);

        builder.Property(x => x.Description).HasMaxLength(Occasion.MaxDescriptionLength);

        builder.Property(x => x.Budget).HasPrecision(10, 2);

        builder.HasOne<OccasionType>()
            .WithMany()
            .HasForeignKey(x => x.OccasionTypeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<User>().WithMany().HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => new { x.UserId, x.OccasionTypeId });
    }
}
