using Lonely.Api.Auth;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSingleton<IAuthService, AuthService>();

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

app.Run();

public partial class Program { }
