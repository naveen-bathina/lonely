using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace Lonely.Api.Tests.Meetup;

public class MeetupTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public MeetupTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    private async Task<string> CreateMatch(string user1, string user2)
    {
        await _client.PostAsJsonAsync($"/api/v1/discover/{user1}/like/{user2}", new { });
        var resp = await _client.PostAsJsonAsync($"/api/v1/discover/{user2}/like/{user1}", new { });
        var body = await resp.Content.ReadFromJsonAsync<Dictionary<string, object>>();
        return body!["matchId"].ToString()!;
    }

    // ── Tracer bullet ────────────────────────────────────────────────────────

    [Fact]
    public async Task ProposeMatch_ValidMatch_Returns201WithPendingState()
    {
        var user1 = Guid.NewGuid().ToString();
        var user2 = Guid.NewGuid().ToString();
        var matchId = await CreateMatch(user1, user2);

        var response = await _client.PostAsJsonAsync("/api/v1/meetups",
            new { proposerId = user1, matchId });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<Dictionary<string, object>>();
        Assert.NotNull(body);
        Assert.True(body!.ContainsKey("proposalId"));
        Assert.Equal("pending", body["state"].ToString());
    }

    [Fact]
    public async Task GetProposal_ExistingProposal_ReturnsState()
    {
        var user1 = Guid.NewGuid().ToString();
        var user2 = Guid.NewGuid().ToString();
        var matchId = await CreateMatch(user1, user2);

        var created = await _client.PostAsJsonAsync("/api/v1/meetups",
            new { proposerId = user1, matchId });
        var createdBody = await created.Content.ReadFromJsonAsync<Dictionary<string, object>>();
        var proposalId = createdBody!["proposalId"].ToString()!;

        var response = await _client.GetAsync($"/api/v1/meetups/{proposalId}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<Dictionary<string, object>>();
        Assert.Equal("pending", body!["state"].ToString());
        Assert.Equal(matchId, body["matchId"].ToString());
    }

    [Fact]
    public async Task AcceptProposal_ValidProposal_ReturnsAccepted()
    {
        var user1 = Guid.NewGuid().ToString();
        var user2 = Guid.NewGuid().ToString();
        var matchId = await CreateMatch(user1, user2);

        var created = await _client.PostAsJsonAsync("/api/v1/meetups",
            new { proposerId = user1, matchId });
        var createdBody = await created.Content.ReadFromJsonAsync<Dictionary<string, object>>();
        var proposalId = createdBody!["proposalId"].ToString()!;

        var response = await _client.PatchAsync($"/api/v1/meetups/{proposalId}/accept", null);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<Dictionary<string, object>>();
        Assert.Equal("accepted", body!["state"].ToString());
    }

    [Fact]
    public async Task DeclineProposal_ValidProposal_ReturnsDeclined()
    {
        var user1 = Guid.NewGuid().ToString();
        var user2 = Guid.NewGuid().ToString();
        var matchId = await CreateMatch(user1, user2);

        var created = await _client.PostAsJsonAsync("/api/v1/meetups",
            new { proposerId = user1, matchId });
        var createdBody = await created.Content.ReadFromJsonAsync<Dictionary<string, object>>();
        var proposalId = createdBody!["proposalId"].ToString()!;

        var response = await _client.PatchAsync($"/api/v1/meetups/{proposalId}/decline", null);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<Dictionary<string, object>>();
        Assert.Equal("declined", body!["state"].ToString());
    }

    [Fact]
    public async Task ProposeMatch_DuplicateActiveProposal_Returns409()
    {
        var user1 = Guid.NewGuid().ToString();
        var user2 = Guid.NewGuid().ToString();
        var matchId = await CreateMatch(user1, user2);

        await _client.PostAsJsonAsync("/api/v1/meetups", new { proposerId = user1, matchId });
        var response = await _client.PostAsJsonAsync("/api/v1/meetups",
            new { proposerId = user2, matchId });

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task ExpireProposal_ValidProposal_ReturnsExpired()
    {
        var user1 = Guid.NewGuid().ToString();
        var user2 = Guid.NewGuid().ToString();
        var matchId = await CreateMatch(user1, user2);

        var created = await _client.PostAsJsonAsync("/api/v1/meetups",
            new { proposerId = user1, matchId });
        var createdBody = await created.Content.ReadFromJsonAsync<Dictionary<string, object>>();
        var proposalId = createdBody!["proposalId"].ToString()!;

        var response = await _client.PatchAsync($"/api/v1/meetups/{proposalId}/expire", null);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<Dictionary<string, object>>();
        Assert.Equal("expired", body!["state"].ToString());
    }

    [Fact]
    public async Task ProposeMatch_AfterDeclined_AllowsNewProposal()
    {
        var user1 = Guid.NewGuid().ToString();
        var user2 = Guid.NewGuid().ToString();
        var matchId = await CreateMatch(user1, user2);

        var first = await _client.PostAsJsonAsync("/api/v1/meetups", new { proposerId = user1, matchId });
        var firstBody = await first.Content.ReadFromJsonAsync<Dictionary<string, object>>();
        var proposalId = firstBody!["proposalId"].ToString()!;

        await _client.PatchAsync($"/api/v1/meetups/{proposalId}/decline", null);

        var second = await _client.PostAsJsonAsync("/api/v1/meetups", new { proposerId = user2, matchId });

        Assert.Equal(HttpStatusCode.Created, second.StatusCode);
        var secondBody = await second.Content.ReadFromJsonAsync<Dictionary<string, object>>();
        Assert.Equal("pending", secondBody!["state"].ToString());
    }
}
