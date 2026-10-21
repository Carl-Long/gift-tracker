
using GiftTracker.Domain.Entities;

namespace GiftTracker.Domain.Tests.Entities;

public class UserTests
{
    [Fact]
    public void Constructor_WithValidDetails_CreatesUser()
    {
        // Arrange
        const string identityProvider = "http://localhost:8080/realms/gift-tracker";
        var externalSubjectId = Guid.NewGuid().ToString();

        // Act
        var user = new User(identityProvider, externalSubjectId);

        // Assert
        Assert.Equal(identityProvider, user.IdentityProvider);
        Assert.Equal(externalSubjectId, user.ExternalSubjectId);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_WithInvalidIdentityProvider_ThrowsArgumentException(string identityProvider)
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => new User(identityProvider, "valid-subject-id"));

        Assert.Equal(nameof(identityProvider), exception.ParamName);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_WithInvalidExternalSubjectId_ThrowsArgumentException(string externalSubjectId)
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => new User("valid-provider", externalSubjectId));

        Assert.Equal(nameof(externalSubjectId), exception.ParamName);
    }

    [Fact]
    public void Constructor_WithIdentityProviderExceedingMaxLength_ThrowsArgumentException()
    {
        // Arrange
        var identityProvider = new string('A', User.MaxIdentityProviderLength + 1);

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => new User(identityProvider, "valid-subject-id"));

        Assert.Equal(nameof(identityProvider), exception.ParamName);
    }

    [Fact]
    public void Constructor_WithExternalSubjectIdExceedingMaxLength_ThrowsArgumentException()
    {
        // Arrange
        var externalSubjectId = new string('A', User.MaxExternalSubjectIdLength + 1);

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => new User("valid-provider", externalSubjectId));

        Assert.Equal(nameof(externalSubjectId), exception.ParamName);
    }

    [Fact]
    public void Constructor_WithWhitespace_TrimsValues()
    {
        // Act
        var user = new User("  http://localhost:8080/realms/gift-tracker  ", "  subject-id  ");

        // Assert
        Assert.Equal("http://localhost:8080/realms/gift-tracker", user.IdentityProvider);
        Assert.Equal("subject-id", user.ExternalSubjectId);
    }
}
