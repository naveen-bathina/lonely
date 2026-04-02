namespace Lonely.Api.Profiles;

public record UpsertProfileRequest(
    string Bio,
    string DateOfBirth,
    string[] DatingGoals
);

public record ProfileResponse(
    string UserId,
    string Bio,
    string DateOfBirth,
    string[] DatingGoals,
    bool IsComplete,
    string[] Badges
);

public interface IProfileService
{
    Task<ProfileResponse> Upsert(string userId, UpsertProfileRequest request);
    Task<ProfileResponse?> Get(string userId);
}

public class ProfileService : IProfileService
{
    private readonly Dictionary<string, ProfileResponse> _profiles = new();

    public Task<ProfileResponse> Upsert(string userId, UpsertProfileRequest request)
    {
        var isComplete = !string.IsNullOrWhiteSpace(request.Bio)
            && !string.IsNullOrWhiteSpace(request.DateOfBirth)
            && request.DatingGoals.Length > 0;

        var badges = ComputeBadges(userId, isComplete);

        var profile = new ProfileResponse(
            userId,
            request.Bio,
            request.DateOfBirth,
            request.DatingGoals,
            isComplete,
            badges
        );

        _profiles[userId] = profile;
        return Task.FromResult(profile);
    }

    public Task<ProfileResponse?> Get(string userId)
    {
        _profiles.TryGetValue(userId, out var profile);
        return Task.FromResult(profile);
    }

    private static string[] ComputeBadges(string userId, bool isComplete)
    {
        var badges = new List<string>();
        // New badge: all freshly created profiles
        badges.Add("New");
        return badges.ToArray();
    }
}
