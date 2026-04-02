using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace Lonely.Api.Tests.Messaging;

public class MessagingTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public MessagingTests(WebApplicationFactory<Program> factory)
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

    [Fact]
    public async Task SendMessage_ToValidMatch_Returns201WithMessage()
    {
        var user1 = Guid.NewGuid().ToString();
        var user2 = Guid.NewGuid().ToString();
        var matchId = await CreateMatch(user1, user2);

        var response = await _client.PostAsJsonAsync($"/api/v1/messages/{matchId}",
            new { senderId = user1, text = "Hey there!" });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<Dictionary<string, object>>();
        Assert.NotNull(body);
        Assert.True(body!.ContainsKey("messageId"));
        Assert.Equal("Hey there!", body["text"].ToString());
    }

    [Fact]
    public async Task SendMessage_ToInvalidMatch_Returns404()
    {
        var fakeMatchId = Guid.NewGuid().ToString();

        var response = await _client.PostAsJsonAsync($"/api/v1/messages/{fakeMatchId}",
            new { senderId = Guid.NewGuid().ToString(), text = "Hello?" });

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetThread_ReturnsMessagesInChronologicalOrder()
    {
        var user1 = Guid.NewGuid().ToString();
        var user2 = Guid.NewGuid().ToString();
        var matchId = await CreateMatch(user1, user2);

        await _client.PostAsJsonAsync($"/api/v1/messages/{matchId}", new { senderId = user1, text = "First" });
        await _client.PostAsJsonAsync($"/api/v1/messages/{matchId}", new { senderId = user2, text = "Second" });
        await _client.PostAsJsonAsync($"/api/v1/messages/{matchId}", new { senderId = user1, text = "Third" });

        var response = await _client.GetAsync($"/api/v1/messages/{matchId}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var messages = await response.Content.ReadFromJsonAsync<List<Dictionary<string, object>>>();
        Assert.NotNull(messages);
        Assert.Equal(3, messages!.Count);
        Assert.Equal("First", messages[0]["text"].ToString());
        Assert.Equal("Third", messages[2]["text"].ToString());
    }

    [Fact]
    public async Task ReadReceipts_DisabledByUser_OmitsReadAtFromResponse()
    {
        var user1 = Guid.NewGuid().ToString();
        var user2 = Guid.NewGuid().ToString();
        var matchId = await CreateMatch(user1, user2);

        // user2 disables read receipts
        await _client.PatchAsJsonAsync($"/api/v1/messages/{matchId}/settings",
            new { userId = user2, readReceiptsEnabled = false });

        await _client.PostAsJsonAsync($"/api/v1/messages/{matchId}", new { senderId = user1, text = "Can you see this?" });

        var response = await _client.GetAsync($"/api/v1/messages/{matchId}?viewerId={user1}");
        var messages = await response.Content.ReadFromJsonAsync<List<Dictionary<string, object>>>();

        Assert.NotNull(messages);
        // readAt should be null/absent because recipient (user2) has disabled read receipts
        Assert.False(messages![0].ContainsKey("readAt") && messages[0]["readAt"]?.ToString() != null);
    }

    [Fact]
    public async Task GhostingStatus_AfterUnrepliedMessage_IsIdle()
    {
        var user1 = Guid.NewGuid().ToString();
        var user2 = Guid.NewGuid().ToString();
        var matchId = await CreateMatch(user1, user2);

        await _client.PostAsJsonAsync($"/api/v1/messages/{matchId}", new { senderId = user1, text = "Hello!" });

        // threshold=0 hours means any unreplied message counts as idle
        var response = await _client.GetAsync(
            $"/api/v1/messages/{matchId}/ghosting-status?userId={user1}&thresholdHours=0");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<Dictionary<string, object>>();
        Assert.NotNull(body);
        Assert.Equal("True", body!["isIdle"].ToString(), ignoreCase: true);
    }

    [Fact]
    public async Task GhostingStatus_AfterReply_IsNotIdle()
    {
        var user1 = Guid.NewGuid().ToString();
        var user2 = Guid.NewGuid().ToString();
        var matchId = await CreateMatch(user1, user2);

        await _client.PostAsJsonAsync($"/api/v1/messages/{matchId}", new { senderId = user1, text = "Hello!" });
        await _client.PostAsJsonAsync($"/api/v1/messages/{matchId}", new { senderId = user2, text = "Hi back!" });

        var response = await _client.GetAsync(
            $"/api/v1/messages/{matchId}/ghosting-status?userId={user1}&thresholdHours=0");

        var body = await response.Content.ReadFromJsonAsync<Dictionary<string, object>>();
        Assert.Equal("False", body!["isIdle"].ToString(), ignoreCase: true);
    }

    [Fact]
    public async Task GhostingStatus_OnEmptyThread_IsNotIdle()
    {
        var user1 = Guid.NewGuid().ToString();
        var user2 = Guid.NewGuid().ToString();
        var matchId = await CreateMatch(user1, user2);

        var response = await _client.GetAsync(
            $"/api/v1/messages/{matchId}/ghosting-status?userId={user1}&thresholdHours=0");

        var body = await response.Content.ReadFromJsonAsync<Dictionary<string, object>>();
        Assert.Equal("False", body!["isIdle"].ToString(), ignoreCase: true);
    }
}
