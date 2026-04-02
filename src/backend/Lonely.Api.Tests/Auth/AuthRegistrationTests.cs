using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace Lonely.Api.Tests.Auth;

public class AuthRegistrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public AuthRegistrationTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Register_WithValidEmailAndConsent_Returns201WithUserId()
    {
        var payload = new
        {
            email = "alice@example.com",
            password = "Password1!",
            dateOfBirth = "1990-01-01",
            gdprConsent = true
        };

        var response = await _client.PostAsJsonAsync("/api/v1/auth/register", payload);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<Dictionary<string, object>>();
        Assert.NotNull(body);
        Assert.True(body!.ContainsKey("userId"), "Response must contain a userId");
    }

    [Fact]
    public async Task Register_WithoutGdprConsent_Returns400()
    {
        var payload = new
        {
            email = "bob@example.com",
            password = "Password1!",
            dateOfBirth = "1990-01-01",
            gdprConsent = false
        };

        var response = await _client.PostAsJsonAsync("/api/v1/auth/register", payload);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task ConfirmOtp_WithWrongCode_Returns400()
    {
        var registerPayload = new
        {
            email = "wrongotp@example.com",
            password = "Password1!",
            dateOfBirth = "1990-01-01",
            gdprConsent = true
        };
        var registerResponse = await _client.PostAsJsonAsync("/api/v1/auth/register", registerPayload);
        var registerBody = await registerResponse.Content.ReadFromJsonAsync<Dictionary<string, object>>();
        var userId = registerBody!["userId"].ToString()!;

        var confirmPayload = new { userId, otp = "999999" };
        var response = await _client.PostAsJsonAsync("/api/v1/auth/confirm-otp", confirmPayload);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task TokenRefresh_WithValidRefreshToken_ReturnsNewAccessToken()
    {
        const string email = "refresh@example.com";
        const string password = "Password1!";

        var registerResponse = await _client.PostAsJsonAsync("/api/v1/auth/register",
            new { email, password, dateOfBirth = "1990-01-01", gdprConsent = true });
        var userId = (await registerResponse.Content.ReadFromJsonAsync<Dictionary<string, object>>())!["userId"].ToString()!;
        await _client.PostAsJsonAsync("/api/v1/auth/confirm-otp", new { userId, otp = "000000" });

        var loginResponse = await _client.PostAsJsonAsync("/api/v1/auth/login", new { email, password });
        var loginBody = await loginResponse.Content.ReadFromJsonAsync<Dictionary<string, object>>();
        var refreshToken = loginBody!["refreshToken"].ToString()!;

        var refreshResponse = await _client.PostAsJsonAsync("/api/v1/auth/refresh", new { refreshToken });

        Assert.Equal(HttpStatusCode.OK, refreshResponse.StatusCode);
        var refreshBody = await refreshResponse.Content.ReadFromJsonAsync<Dictionary<string, object>>();
        Assert.NotNull(refreshBody);
        Assert.True(refreshBody!.ContainsKey("accessToken"), "Response must contain a new accessToken");
    }
}

