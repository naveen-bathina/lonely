using Lonely.Api.Auth;
using Lonely.Api.Profiles;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSingleton<IAuthService, AuthService>();
builder.Services.AddSingleton<IProfileService, ProfileService>();

var app = builder.Build();
app.UseHttpsRedirection();

app.MapPost("/api/v1/auth/register", async (RegisterRequest request, IAuthService authService) =>
{
    try
    {
        var result = await authService.Register(request);
        return Results.Created($"/api/v1/users/{result.UserId}", new { userId = result.UserId });
    }
    catch (UnderageException)
    {
        return Results.BadRequest(new { error = "Users must be 18 or older." });
    }
    catch (GdprConsentRequiredException)
    {
        return Results.BadRequest(new { error = "GDPR consent is required." });
    }
});

app.MapPost("/api/v1/auth/confirm-otp", async (ConfirmOtpRequest request, IAuthService authService) =>
{
    try
    {
        await authService.ConfirmOtp(request);
        return Results.Ok(new { message = "Account confirmed." });
    }
    catch (InvalidOtpException)
    {
        return Results.BadRequest(new { error = "Invalid or expired OTP." });
    }
});

app.MapPost("/api/v1/auth/refresh", async (RefreshRequest request, IAuthService authService) =>
{
    try
    {
        var accessToken = await authService.Refresh(request);
        return Results.Ok(new { accessToken });
    }
    catch
    {
        return Results.Unauthorized();
    }
});

app.MapPost("/api/v1/auth/login", async (LoginRequest request, IAuthService authService) =>
{
    try
    {
        var result = await authService.Login(request);
        return Results.Ok(new { accessToken = result.AccessToken, refreshToken = result.RefreshToken });
    }
    catch (InvalidCredentialsException)
    {
        return Results.Unauthorized();
    }
});

app.MapPut("/api/v1/profiles/{userId}", async (string userId, UpsertProfileRequest request, IProfileService profileService) =>
{
    var profile = await profileService.Upsert(userId, request);
    return Results.Ok(new { userId = profile.UserId, isComplete = profile.IsComplete, badges = profile.Badges });
});

app.MapGet("/api/v1/profiles/{userId}", async (string userId, IProfileService profileService) =>
{
    var profile = await profileService.Get(userId);
    return profile is null ? Results.NotFound() : Results.Ok(profile);
});

app.MapPatch("/api/v1/profiles/{userId}/photos/{photoId}/status",
    async (string userId, string photoId, PhotoStatusRequest request, IProfileService profileService) =>
    {
        var photo = await profileService.UpdatePhotoStatus(userId, photoId, request.Status);
        return Results.Ok(new { photo.PhotoId, photo.Status });
    });

app.MapGet("/api/v1/profiles/{userId}/photos", async (string userId, bool? visible, IProfileService profileService) =>
{
    var photos = await profileService.GetPhotos(userId, visible ?? false);
    return Results.Ok(photos.Select(p => new { photoId = p.PhotoId, status = p.Status }));
});

app.MapGet("/api/v1/discover", async (IProfileService profileService) =>
{
    var feed = await profileService.GetDiscoverFeed();
    return Results.Ok(feed);
});

app.MapPost("/api/v1/profiles/{userId}/photos", async (string userId, PhotoUploadRequest request, IProfileService profileService) =>
{
    var photo = await profileService.UploadPhoto(userId, request);
    return Results.Accepted($"/api/v1/profiles/{userId}/photos/{photo.PhotoId}",
        new { photoId = photo.PhotoId, status = photo.Status });
});

app.Run();

public partial class Program { }
