using GiftTracker.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace GiftTracker.Application.Abstractions.Persistence;

public interface IGiftTrackerDbContext
{
    DbSet<User> Users { get; }
    DbSet<Recipient> Recipients { get; }
    DbSet<OccasionType> OccasionTypes { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
