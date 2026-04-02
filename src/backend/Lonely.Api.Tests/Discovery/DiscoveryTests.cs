using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace Lonely.Api.Tests.Discovery;

public class DiscoveryTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public DiscoveryTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    private async Task CreateCompleteProfile(string userId, string dob = "1993-06-15")
    {
        await _client.PutAsJsonAsync($"/api/v1/profiles/{userId}", new
        {
            bio = $"Bio for {userId}",
            dateOfBirth = dob,
            datingGoals = new[] { "long-term" }
        });
    }

    [Fact]
    public async Task GetRecommendations_ReturnsCompleteProfiles()
    {
        var viewerId = Guid.NewGuid().ToString();
        var candidateId = Guid.NewGuid().ToString();
        await CreateCompleteProfile(candidateId);

        var response = await _client.GetAsync($"/api/v1/discover/recommendations?userId={viewerId}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<List<Dictionary<string, object>>>();
        Assert.NotNull(body);
        Assert.Contains(body!, p => p["userId"].ToString() == candidateId);
    }

    [Fact]
    public async Task GetRecommendations_ExcludesViewerFromOwnFeed()
    {
        var viewerId = Guid.NewGuid().ToString();
        await CreateCompleteProfile(viewerId);

        var response = await _client.GetAsync($"/api/v1/discover/recommendations?userId={viewerId}");
        var body = await response.Content.ReadFromJsonAsync<List<Dictionary<string, object>>>();

        Assert.DoesNotContain(body!, p => p["userId"].ToString() == viewerId);
    }

    [Fact]
    public async Task SetPreferences_AgeFilter_ExcludesOutOfRangeProfiles()
    {
        var viewerId = Guid.NewGuid().ToString();
        var youngId = Guid.NewGuid().ToString();
        await CreateCompleteProfile(youngId, "2004-01-01"); // ~22 years old in 2026

        var prefResponse = await _client.PostAsJsonAsync($"/api/v1/discover/{viewerId}/preferences",
            new { minAge = 30, maxAge = 40, location = "NYC", interests = new[] { "hiking" } });
        Assert.Equal(HttpStatusCode.OK, prefResponse.StatusCode);

        var response = await _client.GetAsync($"/api/v1/discover/recommendations?userId={viewerId}");
        var body = await response.Content.ReadFromJsonAsync<List<Dictionary<string, object>>>();
        Assert.DoesNotContain(body!, p => p["userId"].ToString() == youngId);
    }

    [Fact]
    public async Task SaveQuestionnaire_Returns200()
    {
        var userId = Guid.NewGuid().ToString();
        var payload = new
        {
            answers = new Dictionary<string, string>
            {
                ["communication_style"] = "direct",
                ["activity_level"] = "active"
            }
        };

        var response = await _client.PostAsJsonAsync($"/api/v1/profiles/{userId}/questionnaire", payload);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Like_WhenMutual_CreatesMatch()
    {
        var user1 = Guid.NewGuid().ToString();
        var user2 = Guid.NewGuid().ToString();

        await _client.PostAsJsonAsync($"/api/v1/discover/{user1}/like/{user2}", new { });
        var response = await _client.PostAsJsonAsync($"/api/v1/discover/{user2}/like/{user1}", new { });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<Dictionary<string, object>>();
        Assert.NotNull(body);
        Assert.True(body!.ContainsKey("matchId"));
    }

    [Fact]
    public async Task Like_WhenNotMutual_Returns204()
    {
        var user1 = Guid.NewGuid().ToString();
        var user2 = Guid.NewGuid().ToString();

        var response = await _client.PostAsJsonAsync($"/api/v1/discover/{user1}/like/{user2}", new { });

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task Pass_ExcludesProfileFromRecommendations()
    {
        var viewerId = Guid.NewGuid().ToString();
        var targetId = Guid.NewGuid().ToString();
        await CreateCompleteProfile(targetId);

        await _client.PostAsJsonAsync($"/api/v1/discover/{viewerId}/pass/{targetId}", new { });
        var response = await _client.GetAsync($"/api/v1/discover/recommendations?userId={viewerId}");
        var body = await response.Content.ReadFromJsonAsync<List<Dictionary<string, object>>>();

        Assert.DoesNotContain(body!, p => p["userId"].ToString() == targetId);
    }

    [Fact]
    public async Task BlockedUser_HiddenFromRecommendations()
    {
        var viewerId = Guid.NewGuid().ToString();
        var blockedId = Guid.NewGuid().ToString();
        await CreateCompleteProfile(blockedId);

        await _client.PostAsJsonAsync($"/api/v1/users/{viewerId}/block/{blockedId}", new { });
        var response = await _client.GetAsync($"/api/v1/discover/recommendations?userId={viewerId}");
        var body = await response.Content.ReadFromJsonAsync<List<Dictionary<string, object>>>();

        Assert.DoesNotContain(body!, p => p["userId"].ToString() == blockedId);
    }
}
