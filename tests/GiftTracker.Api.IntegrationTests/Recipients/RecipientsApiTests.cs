using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using GiftTracker.Api.IntegrationTests.Infrastructure;
using Microsoft.AspNetCore.Mvc.Testing;

namespace GiftTracker.Api.IntegrationTests.Recipients;

public sealed class RecipientsApiTests(ApiTestFixture fixture) : IClassFixture<ApiTestFixture>
{
    private const string BaseUrl = "/api/v1/recipients";

    [Fact]
    public async Task CreateRecipient_ReturnsCreatedWithWorkingLocation()
    {
        // Arrange
        using var client = CreateClient(NewUserId());

        // Act
        var response = await client.PostAsJsonAsync(
            BaseUrl,
            new
            {
                name = "Mum",
                description = "Birthday gifts",
                birthDate = "1970-05-15",
                relationship = "ImmediateFamily"
            });

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.NotNull(response.Headers.Location);

        var getResponse = await client.GetAsync(response.Headers.Location);

        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);

        var recipient = await getResponse.Content.ReadFromJsonAsync<JsonElement>();

        Assert.Equal("Mum", recipient.GetProperty("name").GetString());
        Assert.Equal("ImmediateFamily", recipient.GetProperty("relationship").GetString());
        Assert.Equal("1970-05-15", recipient.GetProperty("birthDate").GetString());
    }

    [Fact]
    public async Task GetRecipients_WithoutAuthentication_ReturnsUnauthorized()
    {
        // Arrange
        using var client = CreateClient();

        // Act
        var response = await client.GetAsync(BaseUrl);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetRecipientById_WhenMissing_ReturnsProblemDetails()
    {
        // Arrange
        using var client = CreateClient(NewUserId());

        var recipientId = Guid.NewGuid();

        // Act
        var response = await client.GetAsync($"{BaseUrl}/{recipientId}");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);

        var problem = await response.Content.ReadFromJsonAsync<JsonElement>();

        Assert.Equal(404, problem.GetProperty("status").GetInt32());
        Assert.Equal("Not Found", problem.GetProperty("title").GetString());
        Assert.Equal("Recipient not found.", problem.GetProperty("detail").GetString());
    }

    [Fact]
    public async Task CreateRecipient_WithWhitespaceName_ReturnsBadRequest()
    {
        // Arrange
        using var client = CreateClient(NewUserId());

        // Act
        var response = await client.PostAsJsonAsync(BaseUrl, new { name = "   " });

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var problem = await response.Content.ReadFromJsonAsync<JsonElement>();

        Assert.Equal(400, problem.GetProperty("status").GetInt32());

        Assert.True(problem.TryGetProperty("errors", out _));
    }

    [Fact]
    public async Task RecipientOwnedByAnotherUser_IsNotAccessible()
    {
        // Arrange
        using var userAClient = CreateClient(NewUserId());
        using var userBClient = CreateClient(NewUserId());

        var recipientId = await CreateRecipientAsync(userAClient, "Mum");

        var recipientUrl = $"{BaseUrl}/{recipientId}";

        // Act
        var getResponse = await userBClient.GetAsync(recipientUrl);

        var updateResponse = await userBClient.PutAsJsonAsync(recipientUrl, new { name = "Changed name" });

        var archiveResponse = await userBClient.PatchAsJsonAsync(recipientUrl, new { isArchived = true });

        var deleteResponse = await userBClient.DeleteAsync(recipientUrl);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, updateResponse.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, archiveResponse.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, deleteResponse.StatusCode);

        // The original owner can still access the recipient.
        var ownerResponse = await userAClient.GetAsync(recipientUrl);

        Assert.Equal(HttpStatusCode.OK, ownerResponse.StatusCode);

        var recipient = await ownerResponse.Content.ReadFromJsonAsync<JsonElement>();

        Assert.Equal("Mum", recipient.GetProperty("name").GetString());
        Assert.False(recipient.GetProperty("isArchived").GetBoolean());
    }

    [Fact]
    public async Task ArchiveRestoreAndDeleteRecipient_UpdatesResourceState()
    {
        // Arrange
        using var client = CreateClient(NewUserId());

        var recipientId = await CreateRecipientAsync(client, "Mum");

        var recipientUrl = $"{BaseUrl}/{recipientId}";

        // Archive
        var archiveResponse = await client.PatchAsJsonAsync(recipientUrl, new { isArchived = true });

        Assert.Equal(HttpStatusCode.NoContent, archiveResponse.StatusCode);

        var activeList = await client.GetFromJsonAsync<JsonElement>(BaseUrl);

        Assert.Empty(activeList.EnumerateArray());

        var allRecipients = await client.GetFromJsonAsync<JsonElement>($"{BaseUrl}?includeArchived=true");

        Assert.Single(allRecipients.EnumerateArray());

        // Restore
        var restoreResponse = await client.PatchAsJsonAsync(recipientUrl, new { isArchived = false });

        Assert.Equal(HttpStatusCode.NoContent, restoreResponse.StatusCode);

        var restoredList = await client.GetFromJsonAsync<JsonElement>(BaseUrl);

        Assert.Single(restoredList.EnumerateArray());

        // Delete
        var deleteResponse = await client.DeleteAsync(recipientUrl);

        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

        var getResponse = await client.GetAsync(recipientUrl);

        Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
    }


    [Fact]
    public async Task UpdateRecipient_WithValidDetails_PersistsChanges()
    {
        // Arrange
        using var client = CreateClient(NewUserId());

        var recipientId = await CreateRecipientAsync(client, "Mum");
        var recipientUrl = $"{BaseUrl}/{recipientId}";

        // Act
        var updateResponse = await client.PutAsJsonAsync(
            recipientUrl,
            new
            {
                name = "Mother",
                description = "Updated gift preferences",
                birthDate = "1970-05-15",
                relationship = "ImmediateFamily"
            });

        // Assert
        Assert.Equal(HttpStatusCode.OK, updateResponse.StatusCode);

        var getResponse = await client.GetAsync(recipientUrl);

        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);

        var recipient = await getResponse.Content.ReadFromJsonAsync<JsonElement>();

        Assert.Equal(recipientId, recipient.GetProperty("id").GetGuid());
        Assert.Equal("Mother", recipient.GetProperty("name").GetString());
        Assert.Equal("Updated gift preferences", recipient.GetProperty("description").GetString());
        Assert.Equal("1970-05-15", recipient.GetProperty("birthDate").GetString());
        Assert.Equal("ImmediateFamily", recipient.GetProperty("relationship").GetString());
    }

    [Fact]
    public async Task UpdateRecipientArchiveStatus_WithoutIsArchived_ReturnsBadRequest()
    {
        // Arrange
        using var client = CreateClient(NewUserId());

        var recipientId = await CreateRecipientAsync(client, "Mum");
        var recipientUrl = $"{BaseUrl}/{recipientId}";

        using var content = new StringContent("{}", System.Text.Encoding.UTF8, "application/json");

        // Act
        var response = await client.PatchAsync(recipientUrl, content);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var problem = await response.Content.ReadFromJsonAsync<JsonElement>();

        Assert.Equal(400, problem.GetProperty("status").GetInt32());
        Assert.True(problem.TryGetProperty("errors", out _));

        // Verify that the recipient was not changed.
        var recipient = await client.GetFromJsonAsync<JsonElement>(recipientUrl);

        Assert.False(recipient.GetProperty("isArchived").GetBoolean());
    }

    // TEST HELPERS

    private HttpClient CreateClient(string? userId = null)
    {
        var client = fixture.Factory.CreateClient(
            new WebApplicationFactoryClientOptions
            {
                BaseAddress = new Uri("https://localhost")
            });

        if (userId is not null)
        {
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
                TestAuthenticationHandler.SchemeName, userId);
        }

        return client;
    }

    private static string NewUserId()
    {
        return Guid.NewGuid().ToString("N");
    }

    private static async Task<Guid> CreateRecipientAsync(HttpClient client, string name)
    {
        var response = await client.PostAsJsonAsync(BaseUrl, new { name });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var recipient = await response.Content.ReadFromJsonAsync<JsonElement>();

        return recipient.GetProperty("id").GetGuid();
    }
}
