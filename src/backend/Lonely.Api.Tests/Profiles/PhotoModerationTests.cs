using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace Lonely.Api.Tests.Profiles;

public class PhotoModerationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public PhotoModerationTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    private async Task<string> UploadPhoto(string userId)
    {
        var response = await _client.PostAsJsonAsync($"/api/v1/profiles/{userId}/photos",
            new { fileName = "photo.jpg", contentType = "image/jpeg" });
        var body = await response.Content.ReadFromJsonAsync<Dictionary<string, object>>();
        return body!["photoId"].ToString()!;
    }

    [Fact]
    public async Task ApprovePhoto_MakesItVisible()
    {
        var userId = Guid.NewGuid().ToString();
        var photoId = await UploadPhoto(userId);

        var response = await _client.PatchAsJsonAsync(
            $"/api/v1/profiles/{userId}/photos/{photoId}/status",
            new { status = "approved" });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var photosResponse = await _client.GetAsync($"/api/v1/profiles/{userId}/photos?visible=true");
        var photos = await photosResponse.Content.ReadFromJsonAsync<List<Dictionary<string, object>>>();
        Assert.Contains(photos!, p => p["photoId"].ToString() == photoId);
    }

    [Fact]
    public async Task FlagPhoto_ExcludesItFromVisiblePhotos()
    {
        var userId = Guid.NewGuid().ToString();
        var photoId = await UploadPhoto(userId);

        await _client.PatchAsJsonAsync(
            $"/api/v1/profiles/{userId}/photos/{photoId}/status",
            new { status = "flagged" });

        var photosResponse = await _client.GetAsync($"/api/v1/profiles/{userId}/photos?visible=true");
        var photos = await photosResponse.Content.ReadFromJsonAsync<List<Dictionary<string, object>>>();
        Assert.DoesNotContain(photos!, p => p["photoId"].ToString() == photoId);
    }
}
