using GiftTracker.Application.Abstractions.Persistence;
using GiftTracker.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace GiftTracker.Infrastructure.Persistence;

public class GiftTrackerDbContext(DbContextOptions<GiftTrackerDbContext> options)
    : DbContext(options), IGiftTrackerDbContext
{
    public DbSet<User> Users => Set<User>();
    public DbSet<Recipient> Recipients => Set<Recipient>();
    public DbSet<OccasionType> OccasionTypes => Set<OccasionType>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(GiftTrackerDbContext).Assembly);
    }
}
