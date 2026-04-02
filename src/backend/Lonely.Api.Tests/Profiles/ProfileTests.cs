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
}
