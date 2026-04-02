using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace Lonely.Api.Auth;

public record RegisterRequest(
    string Email,
    string Password,
    string DateOfBirth,
    bool GdprConsent
);

public record RegistrationResult(string UserId);

public record ConfirmOtpRequest(string UserId, string Otp);

public record LoginRequest(string Email, string Password);

public record LoginResult(string AccessToken, string RefreshToken);

public record RefreshRequest(string RefreshToken);

public interface IAuthService
{
    Task<RegistrationResult> Register(RegisterRequest request);
    Task ConfirmOtp(ConfirmOtpRequest request);
    Task<LoginResult> Login(LoginRequest request);
    Task<string> Refresh(RefreshRequest request);
}

public class AuthService : IAuthService
{
    private const string JwtSecret = "lonely-test-secret-key-minimum-32-chars!!";

    private readonly Dictionary<string, string> _pendingOtps = new();
    private readonly Dictionary<string, (string PasswordHash, bool Confirmed)> _users = new();

    public Task<RegistrationResult> Register(RegisterRequest request)
    {
        if (!request.GdprConsent)
            throw new GdprConsentRequiredException();

        if (!DateOnly.TryParse(request.DateOfBirth, out var dob))
            throw new ArgumentException("Invalid date of birth format.");

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var age = today.Year - dob.Year;
        if (dob > today.AddYears(-age)) age--;

        if (age < 18)
            throw new UnderageException();

        var userId = Guid.NewGuid().ToString();
        _pendingOtps[userId] = "000000";
        _users[request.Email] = (request.Password, false);
        // Store userId→email mapping for login lookup
        _userIdToEmail[userId] = request.Email;
        _emailToUserId[request.Email] = userId;

        return Task.FromResult(new RegistrationResult(userId));
    }

    private readonly Dictionary<string, string> _userIdToEmail = new();
    private readonly Dictionary<string, string> _emailToUserId = new();

    public Task ConfirmOtp(ConfirmOtpRequest request)
    {
        if (!_pendingOtps.TryGetValue(request.UserId, out var expected) || expected != request.Otp)
            throw new InvalidOtpException();

        _pendingOtps.Remove(request.UserId);

        if (_userIdToEmail.TryGetValue(request.UserId, out var email) && _users.TryGetValue(email, out var user))
            _users[email] = (user.PasswordHash, Confirmed: true);

        return Task.CompletedTask;
    }

    private readonly Dictionary<string, string> _refreshTokens = new(); // refreshToken → userId

    public Task<LoginResult> Login(LoginRequest request)
    {
        if (!_users.TryGetValue(request.Email, out var user) ||
            user.PasswordHash != request.Password ||
            !user.Confirmed)
            throw new InvalidCredentialsException();

        var userId = _emailToUserId[request.Email];
        var accessToken = GenerateJwt(userId, request.Email, TimeSpan.FromMinutes(15));
        var refreshToken = Guid.NewGuid().ToString("N");
        _refreshTokens[refreshToken] = userId;

        return Task.FromResult(new LoginResult(accessToken, refreshToken));
    }

    public Task<string> Refresh(RefreshRequest request)
    {
        if (!_refreshTokens.TryGetValue(request.RefreshToken, out var userId))
            throw new InvalidOtpException();

        var email = _userIdToEmail[userId];
        var newAccessToken = GenerateJwt(userId, email, TimeSpan.FromMinutes(15));
        return Task.FromResult(newAccessToken);
    }

    private static string GenerateJwt(string userId, string email, TimeSpan lifetime)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(JwtSecret));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var claims = new[] { new Claim(JwtRegisteredClaimNames.Sub, userId), new Claim(JwtRegisteredClaimNames.Email, email) };
        var token = new JwtSecurityToken(claims: claims, expires: DateTime.UtcNow.Add(lifetime), signingCredentials: creds);
        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}

public class GdprConsentRequiredException : Exception
{
    public GdprConsentRequiredException() : base("GDPR consent is required.") { }
}

public class UnderageException : Exception
{
    public UnderageException() : base("Users must be 18 or older.") { }
}

public class InvalidOtpException : Exception
{
    public InvalidOtpException() : base("Invalid or expired OTP.") { }
}

public class InvalidCredentialsException : Exception
{
    public InvalidCredentialsException() : base("Invalid email, password, or unconfirmed account.") { }
}
