using GiftTracker.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GiftTracker.Infrastructure.Persistence.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.IdentityProvider)
            .IsRequired()
            .HasMaxLength(User.MaxIdentityProviderLength);

        builder.Property(x => x.ExternalSubjectId)
            .IsRequired()
            .HasMaxLength(User.MaxExternalSubjectIdLength);

        builder.HasIndex(x => new { x.IdentityProvider, x.ExternalSubjectId })
            .IsUnique();
    }
}
