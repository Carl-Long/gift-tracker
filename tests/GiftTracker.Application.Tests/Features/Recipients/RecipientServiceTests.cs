using GiftTracker.Application.Common.Exceptions;
using GiftTracker.Application.Features.Recipients.Dtos;
using GiftTracker.Application.Features.Recipients.Services;
using GiftTracker.Application.Tests.TestHelpers;
using GiftTracker.Domain.Entities;
using GiftTracker.Domain.Enums;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace GiftTracker.Application.Tests.Features.Recipients;

public sealed class RecipientServiceTests : IDisposable
{
    private const string IdentityProvider = "http://localhost:8080/realms/gift-tracker";

    private readonly SqliteConnection _connection;
    private readonly TestDbContext _dbContext;

    public RecipientServiceTests()
    {
        _connection = new SqliteConnection("Data Source=:memory:");
        _connection.Open();

        var options = new DbContextOptionsBuilder<TestDbContext>().UseSqlite(_connection).Options;

        _dbContext = new TestDbContext(options);
        _dbContext.Database.EnsureCreated();
    }

    // GET ALL

    [Fact]
    public async Task GetRecipientsAsync_ReturnsOnlyCurrentUsersActiveRecipients()
    {
        // Arrange
        var currentUserId = await SeedUserAsync("user-1");
        var otherUserId = await SeedUserAsync("user-2");

        var myRecipient = await SeedRecipientAsync(currentUserId, "Mum");

        await SeedRecipientAsync(currentUserId, "Dad", isArchived: true);
        await SeedRecipientAsync(otherUserId, "Friend");

        var service = CreateService(currentUserId);

        // Act
        var result = await service.GetRecipientsAsync();

        // Assert
        var recipient = Assert.Single(result);

        Assert.Equal(myRecipient.Id, recipient.Id);
        Assert.Equal("Mum", recipient.Name);
    }

    [Fact]
    public async Task GetRecipientsAsync_WithIncludeArchived_ReturnsArchivedRecipients()
    {
        // Arrange
        var userId = await SeedUserAsync("user-1");

        var activeRecipient = await SeedRecipientAsync(userId, "Mum");
        var archivedRecipient = await SeedRecipientAsync(userId, "Dad", isArchived: true);

        var service = CreateService(userId);

        // Act
        var result = await service.GetRecipientsAsync(includeArchived: true);

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Contains(result, r => r.Id == activeRecipient.Id);
        Assert.Contains(result, r => r.Id == archivedRecipient.Id);
    }

    [Fact]
    public async Task GetRecipientsAsync_ReturnsRecipientsOrderedByName()
    {
        // Arrange
        var userId = await SeedUserAsync("user-1");

        await SeedRecipientAsync(userId, "Mum");
        await SeedRecipientAsync(userId, "Dad");

        var service = CreateService(userId);

        // Act
        var result = await service.GetRecipientsAsync();

        // Assert
        Assert.Equal(new[] { "Dad", "Mum" }, result.Select(r => r.Name).ToArray());
    }

    // GET BY ID

    [Fact]
    public async Task GetRecipientByIdAsync_WithOwnedRecipient_ReturnsRecipient()
    {
        // Arrange
        var userId = await SeedUserAsync("user-1");

        var recipient = await SeedRecipientAsync(userId, "Mum", description: "Birthday gifts");

        var service = CreateService(userId);

        // Act
        var result = await service.GetRecipientByIdAsync(recipient.Id);

        // Assert
        Assert.Equal(recipient.Id, result.Id);
        Assert.Equal("Mum", result.Name);
        Assert.Equal("Birthday gifts", result.Description);
    }

    [Fact]
    public async Task GetRecipientByIdAsync_WithOtherUsersRecipient_ThrowsNotFoundException()
    {
        // Arrange
        var currentUserId = await SeedUserAsync("user-1");
        var otherUserId = await SeedUserAsync("user-2");

        var recipient = await SeedRecipientAsync(otherUserId, "Dad");

        var service = CreateService(currentUserId);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => service.GetRecipientByIdAsync(recipient.Id));
    }

    [Fact]
    public async Task GetRecipientByIdAsync_WithMissingRecipient_ThrowsNotFoundException()
    {
        // Arrange
        var userId = await SeedUserAsync("user-1");
        var service = CreateService(userId);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => service.GetRecipientByIdAsync(Guid.NewGuid()));
    }

    // CREATE

    [Fact]
    public async Task CreateRecipientAsync_SavesRecipientForCurrentUser()
    {
        // Arrange
        var userId = await SeedUserAsync("user-1");
        var service = CreateService(userId);
        var birthDate = new DateOnly(1970, 5, 15);

        var dto = new CreateRecipientDto
        {
            Name = "Mum",
            Description = "Birthday gifts",
            BirthDate = birthDate,
            Relationship = RecipientRelationship.ImmediateFamily
        };

        // Act
        var result = await service.CreateRecipientAsync(dto);

        // Assert
        var savedRecipient = await GetPersistedRecipientAsync(result.Id);

        Assert.Equal(userId, savedRecipient.UserId);
        Assert.Equal("Mum", savedRecipient.Name);
        Assert.Equal("Birthday gifts", savedRecipient.Description);
        Assert.Equal(birthDate, savedRecipient.BirthDate);
        Assert.Equal(RecipientRelationship.ImmediateFamily, savedRecipient.Relationship);
        Assert.False(savedRecipient.IsArchived);
    }

    [Fact]
    public async Task CreateRecipientAsync_WithDuplicateName_CreatesSeparateRecipients()
    {
        // Arrange
        var userId = await SeedUserAsync("user-1");
        var service = CreateService(userId);

        var dto = new CreateRecipientDto { Name = "James" };

        // Act
        var first = await service.CreateRecipientAsync(dto);
        var second = await service.CreateRecipientAsync(dto);

        // Assert
        Assert.NotEqual(first.Id, second.Id);

        _dbContext.ChangeTracker.Clear();

        var count = await _dbContext.Recipients.CountAsync(r => r.UserId == userId && r.Name == "James");

        Assert.Equal(2, count);
    }

    // UPDATE

    [Fact]
    public async Task UpdateRecipientAsync_WithOwnedRecipient_SavesUpdatedDetails()
    {
        // Arrange
        var userId = await SeedUserAsync("user-1");

        var recipient = await SeedRecipientAsync(userId, "Mum");

        var service = CreateService(userId);
        var birthDate = new DateOnly(1970, 5, 15);

        var dto = new UpdateRecipientDto
        {
            Name = "Mother",
            Description = "Updated preferences",
            BirthDate = birthDate,
            Relationship = RecipientRelationship.ImmediateFamily
        };

        // Act
        var result = await service.UpdateRecipientAsync(recipient.Id, dto);

        // Assert
        var savedRecipient = await GetPersistedRecipientAsync(recipient.Id);

        Assert.Equal(recipient.Id, result.Id);
        Assert.Equal("Mother", savedRecipient.Name);
        Assert.Equal("Updated preferences", savedRecipient.Description);
        Assert.Equal(birthDate, savedRecipient.BirthDate);
        Assert.Equal(RecipientRelationship.ImmediateFamily, savedRecipient.Relationship);
    }

    [Fact]
    public async Task UpdateRecipientAsync_WithOmittedOptionalFields_ClearsExistingValues()
    {
        // Arrange
        var userId = await SeedUserAsync("user-1");

        var recipient = await SeedRecipientAsync(
            userId,
            "Mum",
            description: "Old description",
            birthDate: new DateOnly(1970, 5, 15),
            relationship: RecipientRelationship.ImmediateFamily);

        var service = CreateService(userId);

        var dto = new UpdateRecipientDto { Name = "Mum" };

        // Act
        await service.UpdateRecipientAsync(recipient.Id, dto);

        // Assert
        var savedRecipient = await GetPersistedRecipientAsync(recipient.Id);

        Assert.Null(savedRecipient.Description);
        Assert.Null(savedRecipient.BirthDate);
        Assert.Null(savedRecipient.Relationship);
    }

    [Fact]
    public async Task UpdateRecipientAsync_WithOtherUsersRecipient_ThrowsNotFoundException()
    {
        // Arrange
        var currentUserId = await SeedUserAsync("user-1");
        var otherUserId = await SeedUserAsync("user-2");

        var recipient = await SeedRecipientAsync(otherUserId, "Dad");

        var service = CreateService(currentUserId);

        var dto = new UpdateRecipientDto { Name = "Updated name" };

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => service.UpdateRecipientAsync(recipient.Id, dto));

        var savedRecipient = await GetPersistedRecipientAsync(recipient.Id);

        Assert.Equal("Dad", savedRecipient.Name);
    }

    [Fact]
    public async Task UpdateRecipientAsync_WithMissingRecipient_ThrowsNotFoundException()
    {
        // Arrange
        var userId = await SeedUserAsync("user-1");
        var service = CreateService(userId);

        var dto = new UpdateRecipientDto { Name = "Mum" };

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => service.UpdateRecipientAsync(Guid.NewGuid(), dto));
    }

    // ARCHIVE AND RESTORE

    [Fact]
    public async Task SetRecipientArchivedStatusAsync_WithTrue_ArchivesRecipient()
    {
        // Arrange
        var userId = await SeedUserAsync("user-1");
        var recipient = await SeedRecipientAsync(userId, "Mum");

        var service = CreateService(userId);

        // Act
        await service.SetRecipientArchivedStatusAsync(recipient.Id, true);

        // Assert
        var savedRecipient = await GetPersistedRecipientAsync(recipient.Id);

        Assert.True(savedRecipient.IsArchived);
    }

    [Fact]
    public async Task SetRecipientArchivedStatusAsync_WithFalse_RestoresRecipient()
    {
        // Arrange
        var userId = await SeedUserAsync("user-1");

        var recipient = await SeedRecipientAsync(userId, "Mum", isArchived: true);

        var service = CreateService(userId);

        // Act
        await service.SetRecipientArchivedStatusAsync(recipient.Id, false);

        // Assert
        var savedRecipient = await GetPersistedRecipientAsync(recipient.Id);

        Assert.False(savedRecipient.IsArchived);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task SetRecipientArchivedStatusAsync_WhenStatusIsUnchanged_Succeeds(bool isArchived)
    {
        // Arrange
        var userId = await SeedUserAsync("user-1");

        var recipient = await SeedRecipientAsync(userId, "Mum", isArchived: isArchived);

        var service = CreateService(userId);

        // Act
        await service.SetRecipientArchivedStatusAsync(recipient.Id, isArchived);
        await service.SetRecipientArchivedStatusAsync(recipient.Id, isArchived);

        // Assert
        var savedRecipient = await GetPersistedRecipientAsync(recipient.Id);

        Assert.Equal(isArchived, savedRecipient.IsArchived);
    }

    [Fact]
    public async Task SetRecipientArchivedStatusAsync_WithOtherUsersRecipient_ThrowsNotFoundException()
    {
        // Arrange
        var currentUserId = await SeedUserAsync("user-1");
        var otherUserId = await SeedUserAsync("user-2");

        var recipient = await SeedRecipientAsync(otherUserId, "Dad");

        var service = CreateService(currentUserId);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => service.SetRecipientArchivedStatusAsync(recipient.Id, true));

        var savedRecipient = await GetPersistedRecipientAsync(recipient.Id);

        Assert.False(savedRecipient.IsArchived);
    }

    [Fact]
    public async Task SetRecipientArchivedStatusAsync_WithMissingRecipient_ThrowsNotFoundException()
    {
        // Arrange
        var userId = await SeedUserAsync("user-1");
        var service = CreateService(userId);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => service.SetRecipientArchivedStatusAsync(Guid.NewGuid(), true));
    }

    // DELETE

    [Fact]
    public async Task DeleteRecipientAsync_WithOwnedRecipient_RemovesRecipient()
    {
        // Arrange
        var userId = await SeedUserAsync("user-1");
        var recipient = await SeedRecipientAsync(userId, "Mum");

        var service = CreateService(userId);

        // Act
        await service.DeleteRecipientAsync(recipient.Id);

        // Assert
        _dbContext.ChangeTracker.Clear();

        var exists = await _dbContext.Recipients.AnyAsync(r => r.Id == recipient.Id);

        Assert.False(exists);
    }

    [Fact]
    public async Task DeleteRecipientAsync_WithOtherUsersRecipient_ThrowsNotFoundException()
    {
        // Arrange
        var currentUserId = await SeedUserAsync("user-1");
        var otherUserId = await SeedUserAsync("user-2");

        var recipient = await SeedRecipientAsync(otherUserId, "Dad");

        var service = CreateService(currentUserId);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => service.DeleteRecipientAsync(recipient.Id));

        var savedRecipient = await GetPersistedRecipientAsync(recipient.Id);

        Assert.Equal("Dad", savedRecipient.Name);
    }

    [Fact]
    public async Task DeleteRecipientAsync_WithMissingRecipient_ThrowsNotFoundException()
    {
        // Arrange
        var userId = await SeedUserAsync("user-1");
        var service = CreateService(userId);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => service.DeleteRecipientAsync(Guid.NewGuid()));
    }

    // TEST HELPERS

    private RecipientService CreateService(Guid userId)
    {
        var userResolutionService = new FakeUserResolutionService(userId);
        return new RecipientService(_dbContext, userResolutionService);
    }

    private async Task<Guid> SeedUserAsync(string subjectId)
    {
        var user = new User(IdentityProvider, subjectId);

        _dbContext.Users.Add(user);

        await _dbContext.SaveChangesAsync();

        return user.Id;
    }

    private async Task<Recipient> SeedRecipientAsync(
        Guid userId,
        string name,
        bool isArchived = false,
        string? description = null,
        DateOnly? birthDate = null,
        RecipientRelationship? relationship = null)
    {
        var recipient = new Recipient(userId, name, description, birthDate, relationship);

        if (isArchived)
        {
            recipient.Archive();
        }

        _dbContext.Recipients.Add(recipient);

        await _dbContext.SaveChangesAsync();

        return recipient;
    }

    private async Task<Recipient> GetPersistedRecipientAsync(Guid recipientId)
    {
        _dbContext.ChangeTracker.Clear();
        return await _dbContext.Recipients.AsNoTracking().SingleAsync(r => r.Id == recipientId);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
        _connection.Dispose();
    }
}
