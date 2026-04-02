using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace Lonely.Api.Tests.Profiles;

public class ProfileTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public ProfileTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task CreateOrUpdateProfile_WithValidData_Returns200()
    {
        var userId = Guid.NewGuid().ToString();
        var payload = new
        {
            bio = "Love hiking and coffee.",
            dateOfBirth = "1992-06-15",
            datingGoals = new[] { "long-term", "friendship" }
        };

        var response = await _client.PutAsJsonAsync($"/api/v1/profiles/{userId}", payload);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<Dictionary<string, object>>();
        Assert.NotNull(body);
        Assert.True(body!.ContainsKey("userId"));
    }

    [Fact]
    public async Task GetProfile_AfterCreation_ReturnsProfile()
    {
        var userId = Guid.NewGuid().ToString();
        await _client.PutAsJsonAsync($"/api/v1/profiles/{userId}", new
        {
            bio = "Foodie and traveller.",
            dateOfBirth = "1988-03-20",
            datingGoals = new[] { "casual" }
        });

        var response = await _client.GetAsync($"/api/v1/profiles/{userId}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<Dictionary<string, object>>();
        Assert.Equal(userId, body!["userId"].ToString());
    }

    [Fact]
    public async Task GetProfile_NonExistent_Returns404()
    {
        var response = await _client.GetAsync($"/api/v1/profiles/{Guid.NewGuid()}");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task DiscoverFeed_ExcludesIncompleteProfiles()
    {
        // Complete profile
        var completeId = Guid.NewGuid().ToString();
        await _client.PutAsJsonAsync($"/api/v1/profiles/{completeId}", new
        {
            bio = "Complete profile.",
            dateOfBirth = "1990-01-01",
            datingGoals = new[] { "long-term" }
        });

        // Incomplete profile (no bio)
        var incompleteId = Guid.NewGuid().ToString();
        await _client.PutAsJsonAsync($"/api/v1/profiles/{incompleteId}", new
        {
            bio = "",
            dateOfBirth = "1990-01-01",
            datingGoals = new[] { "long-term" }
        });

        var response = await _client.GetAsync("/api/v1/discover");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<List<Dictionary<string, object>>>();
        Assert.NotNull(body);
        Assert.Contains(body!, p => p["userId"].ToString() == completeId);
        Assert.DoesNotContain(body!, p => p["userId"].ToString() == incompleteId);
    }

    [Fact]
    public async Task UploadPhoto_Returns202WithPhotoId()
    {
        var userId = Guid.NewGuid().ToString();
        var payload = new { fileName = "photo.jpg", contentType = "image/jpeg" };

        var response = await _client.PostAsJsonAsync($"/api/v1/profiles/{userId}/photos", payload);

        Assert.Equal(HttpStatusCode.Accepted, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<Dictionary<string, object>>();
        Assert.True(body!.ContainsKey("photoId"));
        Assert.Equal("pending", body["status"].ToString());
    }

    [Fact]
    public async Task GetProfile_ReturnsNewBadge_ForFreshProfile()
    {
        var userId = Guid.NewGuid().ToString();
        await _client.PutAsJsonAsync($"/api/v1/profiles/{userId}", new
        {
            bio = "Fresh user.",
            dateOfBirth = "1995-07-10",
            datingGoals = new[] { "friendship" }
        });

        var response = await _client.GetAsync($"/api/v1/profiles/{userId}");
        var body = await response.Content.ReadFromJsonAsync<Dictionary<string, object>>();

        var badges = body!["badges"].ToString()!;
        Assert.Contains("New", badges);
    }
}
