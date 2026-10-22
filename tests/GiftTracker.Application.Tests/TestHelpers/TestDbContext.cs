using GiftTracker.Application.Abstractions.Persistence;
using GiftTracker.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace GiftTracker.Application.Tests.TestHelpers;

internal sealed class TestDbContext(DbContextOptions<TestDbContext> options) : DbContext(options), IGiftTrackerDbContext
{
    public DbSet<User> Users => Set<User>();
    public DbSet<Recipient> Recipients => Set<Recipient>();
    public DbSet<OccasionType> OccasionTypes => Set<OccasionType>();
}
