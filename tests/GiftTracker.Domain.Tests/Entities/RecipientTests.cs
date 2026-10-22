using GiftTracker.Domain.Entities;
using GiftTracker.Domain.Enums;

namespace GiftTracker.Domain.Tests.Entities;

public class RecipientTests
{
    [Fact]
    public void Constructor_WithValidDetails_CreatesRecipient()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var birthDate = new DateOnly(1970, 5, 15);

        // Act
        var recipient = new Recipient(
            userId,
            "Mum",
            "Christmas and birthday gifts",
            birthDate,
            RecipientRelationship.ImmediateFamily);

        // Assert
        Assert.Equal(userId, recipient.UserId);
        Assert.Equal("Mum", recipient.Name);
        Assert.Equal("Christmas and birthday gifts", recipient.Description);
        Assert.Equal(birthDate, recipient.BirthDate);
        Assert.Equal(RecipientRelationship.ImmediateFamily, recipient.Relationship);
        Assert.False(recipient.IsArchived);
    }

    [Fact]
    public void Constructor_WithEmptyUserId_ThrowsArgumentException()
    {
        // Arrange
        var userId = Guid.Empty;

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => new Recipient(userId, "Mum", null, null, null));

        Assert.Equal(nameof(userId), exception.ParamName);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("   ")]
    public void UpdateDetails_WithInvalidName_ThrowsArgumentException(string name)
    {
        // Arrange
        var recipient = CreateRecipient();

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => recipient.UpdateDetails(name, null, null, null));

        Assert.Equal(nameof(name), exception.ParamName);
    }

    [Fact]
    public void UpdateDetails_WithNameExceedingMaxLength_ThrowsArgumentException()
    {
        // Arrange
        var recipient = CreateRecipient();
        var name = new string('A', Recipient.MaxNameLength + 1);

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => recipient.UpdateDetails(name, null, null, null));

        Assert.Equal(nameof(name), exception.ParamName);
    }

    [Fact]
    public void UpdateDetails_WithValidDetails_UpdatesAndTrimsValues()
    {
        // Arrange
        var recipient = CreateRecipient();
        var birthDate = new DateOnly(1980, 6, 20);

        // Act
        recipient.UpdateDetails("  Dad  ", "  Birthday gifts  ", birthDate, RecipientRelationship.ImmediateFamily);

        // Assert
        Assert.Equal("Dad", recipient.Name);
        Assert.Equal("Birthday gifts", recipient.Description);
        Assert.Equal(birthDate, recipient.BirthDate);
        Assert.Equal(RecipientRelationship.ImmediateFamily, recipient.Relationship);
    }

    [Fact]
    public void UpdateDetails_WithInvalidRelationship_ThrowsAndPreservesDetails()
    {
        // Arrange
        var recipient = CreateRecipient();
        var invalidRelationship = (RecipientRelationship)999;

        // Act & Assert
        var exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
            recipient.UpdateDetails("Dad", "Updated description", null, invalidRelationship));

        Assert.Equal("relationship", exception.ParamName);
        Assert.Equal("Mum", recipient.Name);
        Assert.Null(recipient.Description);
        Assert.Null(recipient.Relationship);
    }

    [Fact]
    public void Archive_WhenActive_SetsIsArchivedToTrue()
    {
        // Arrange
        var recipient = CreateRecipient();

        // Act
        recipient.Archive();

        // Assert
        Assert.True(recipient.IsArchived);
    }

    [Fact]
    public void Restore_WhenArchived_SetsIsArchivedToFalse()
    {
        // Arrange
        var recipient = CreateRecipient();
        recipient.Archive();

        // Act
        recipient.Restore();

        // Assert
        Assert.False(recipient.IsArchived);
    }

    [Fact]
    public void Archive_WhenAlreadyArchived_RemainsArchived()
    {
        // Arrange
        var recipient = new Recipient(Guid.NewGuid(), "Mum", null, null, null);
        recipient.Archive();

        // Act
        recipient.Archive();

        // Assert
        Assert.True(recipient.IsArchived);
    }

    [Fact]
    public void Restore_WhenAlreadyActive_RemainsActive()
    {
        // Arrange
        var recipient = new Recipient(Guid.NewGuid(), "Mum", null, null, null);

        // Act
        recipient.Restore();

        // Assert
        Assert.False(recipient.IsArchived);
    }

    private static Recipient CreateRecipient() =>
        new Recipient(Guid.NewGuid(), "Mum", null, null, null);
}
