using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace Lonely.Api.Tests.Moderation;

public class ModerationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public ModerationTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task SubmitReport_Returns201WithReportId()
    {
        var reporterId = Guid.NewGuid().ToString();
        var targetId = Guid.NewGuid().ToString();

        var response = await _client.PostAsJsonAsync("/api/v1/reports", new
        {
            reporterId,
            targetId,
            reason = "harassment",
            contentId = (string?)null
        });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<Dictionary<string, object>>();
        Assert.NotNull(body);
        Assert.True(body!.ContainsKey("reportId"));
        Assert.Equal("pending", body["status"].ToString());
    }

    [Fact]
    public async Task ModerationQueue_ContainsSubmittedReport()
    {
        var reporterId = Guid.NewGuid().ToString();
        var targetId = Guid.NewGuid().ToString();

        var submitResp = await _client.PostAsJsonAsync("/api/v1/reports",
            new { reporterId, targetId, reason = "spam", contentId = (string?)null });
        var submitted = await submitResp.Content.ReadFromJsonAsync<Dictionary<string, object>>();
        var reportId = submitted!["reportId"].ToString()!;

        var queueResp = await _client.GetAsync("/api/v1/admin/moderation-queue");

        Assert.Equal(HttpStatusCode.OK, queueResp.StatusCode);
        var queue = await queueResp.Content.ReadFromJsonAsync<List<Dictionary<string, object>>>();
        Assert.NotNull(queue);
        Assert.Contains(queue!, r => r["reportId"].ToString() == reportId);
    }

    [Fact]
    public async Task AdminApprove_SetsStatusApproved()
    {
        var reporterId = Guid.NewGuid().ToString();
        var submitResp = await _client.PostAsJsonAsync("/api/v1/reports",
            new { reporterId, targetId = Guid.NewGuid().ToString(), reason = "fake", contentId = (string?)null });
        var submitted = await submitResp.Content.ReadFromJsonAsync<Dictionary<string, object>>();
        var reportId = submitted!["reportId"].ToString()!;

        var approveResp = await _client.PatchAsync(
            $"/api/v1/admin/moderation-queue/{reportId}/approve", null);

        Assert.Equal(HttpStatusCode.OK, approveResp.StatusCode);
        var body = await approveResp.Content.ReadFromJsonAsync<Dictionary<string, object>>();
        Assert.Equal("approved", body!["status"].ToString());
    }

    [Fact]
    public async Task AdminRemove_SetsStatusRemoved()
    {
        var reporterId = Guid.NewGuid().ToString();
        var submitResp = await _client.PostAsJsonAsync("/api/v1/reports",
            new { reporterId, targetId = Guid.NewGuid().ToString(), reason = "nudity", contentId = (string?)null });
        var submitted = await submitResp.Content.ReadFromJsonAsync<Dictionary<string, object>>();
        var reportId = submitted!["reportId"].ToString()!;

        var removeResp = await _client.PatchAsync(
            $"/api/v1/admin/moderation-queue/{reportId}/remove", null);

        Assert.Equal(HttpStatusCode.OK, removeResp.StatusCode);
        var body = await removeResp.Content.ReadFromJsonAsync<Dictionary<string, object>>();
        Assert.Equal("removed", body!["status"].ToString());
    }

    [Fact]
    public async Task ReportHistory_ReturnsOnlyUserReports()
    {
        var userId = Guid.NewGuid().ToString();
        var otherId = Guid.NewGuid().ToString();

        await _client.PostAsJsonAsync("/api/v1/reports",
            new { reporterId = userId, targetId = Guid.NewGuid().ToString(), reason = "spam", contentId = (string?)null });
        await _client.PostAsJsonAsync("/api/v1/reports",
            new { reporterId = otherId, targetId = Guid.NewGuid().ToString(), reason = "spam", contentId = (string?)null });

        var response = await _client.GetAsync($"/api/v1/reports?userId={userId}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var reports = await response.Content.ReadFromJsonAsync<List<Dictionary<string, object>>>();
        Assert.NotNull(reports);
        Assert.All(reports!, r => Assert.Equal(userId, r["reporterId"].ToString()));
    }

    [Fact]
    public async Task ModerationQueue_ShowsOnlyPendingReports()
    {
        var reporterId = Guid.NewGuid().ToString();

        // Submit two reports
        var r1 = await (await _client.PostAsJsonAsync("/api/v1/reports",
            new { reporterId, targetId = Guid.NewGuid().ToString(), reason = "spam", contentId = (string?)null }))
            .Content.ReadFromJsonAsync<Dictionary<string, object>>();
        var r2 = await (await _client.PostAsJsonAsync("/api/v1/reports",
            new { reporterId, targetId = Guid.NewGuid().ToString(), reason = "abuse", contentId = (string?)null }))
            .Content.ReadFromJsonAsync<Dictionary<string, object>>();

        var r1Id = r1!["reportId"].ToString()!;
        var r2Id = r2!["reportId"].ToString()!;

        // Approve r1
        await _client.PatchAsync($"/api/v1/admin/moderation-queue/{r1Id}/approve", null);

        var queueResp = await _client.GetAsync("/api/v1/admin/moderation-queue");
        var queue = await queueResp.Content.ReadFromJsonAsync<List<Dictionary<string, object>>>();

        Assert.NotNull(queue);
        Assert.DoesNotContain(queue!, r => r["reportId"].ToString() == r1Id);
        Assert.Contains(queue!, r => r["reportId"].ToString() == r2Id);
    }
}
