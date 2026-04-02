using Lonely.Api.Profiles;

namespace Lonely.Api.Discovery;

public record SetPreferencesRequest(int MinAge, int MaxAge, string Location, string[] Interests);

public record QuestionnaireRequest(Dictionary<string, string> Answers);

public record RecommendationResponse(string UserId, string Bio, string DateOfBirth, int Score, string[] Badges);

public record MatchResponse(string MatchId, string User1Id, string User2Id);

public interface IDiscoveryService
{
    Task SetPreferences(string userId, SetPreferencesRequest request);
    Task SaveQuestionnaire(string userId, QuestionnaireRequest request);
    Task<IEnumerable<RecommendationResponse>> GetRecommendations(string userId);
    Task<MatchResponse?> Like(string userId, string targetId);
    Task Pass(string userId, string targetId);
    Task Block(string userId, string targetId);
}

public class DiscoveryService : IDiscoveryService
{
    private readonly IProfileService _profileService;
    private readonly Dictionary<string, SetPreferencesRequest> _preferences = new();
    private readonly Dictionary<string, Dictionary<string, string>> _questionnaires = new();
    private readonly Dictionary<string, HashSet<string>> _likes = new();
    private readonly Dictionary<string, HashSet<string>> _passes = new();
    private readonly Dictionary<string, HashSet<string>> _blocks = new();
    private readonly Dictionary<string, MatchResponse> _matches = new();

    public DiscoveryService(IProfileService profileService)
    {
        _profileService = profileService;
    }

    public Task SetPreferences(string userId, SetPreferencesRequest request)
    {
        _preferences[userId] = request;
        return Task.CompletedTask;
    }

    public Task SaveQuestionnaire(string userId, QuestionnaireRequest request)
    {
        _questionnaires[userId] = request.Answers;
        return Task.CompletedTask;
    }

    public async Task<IEnumerable<RecommendationResponse>> GetRecommendations(string userId)
    {
        var allProfiles = (await _profileService.GetDiscoverFeed()).ToList();

        _preferences.TryGetValue(userId, out var prefs);
        _questionnaires.TryGetValue(userId, out var questionnaire);
        _passes.TryGetValue(userId, out var passed);
        _blocks.TryGetValue(userId, out var blocked);

        var blockedByOthers = _blocks
            .Where(kvp => kvp.Value.Contains(userId))
            .Select(kvp => kvp.Key)
            .ToHashSet();

        var results = new List<RecommendationResponse>();

        foreach (var profile in allProfiles)
        {
            if (profile.UserId == userId) continue;
            if (passed?.Contains(profile.UserId) == true) continue;
            if (blocked?.Contains(profile.UserId) == true) continue;
            if (blockedByOthers.Contains(profile.UserId)) continue;

            if (prefs != null && DateTime.TryParse(profile.DateOfBirth, out var dob))
            {
                var age = (int)((DateTime.UtcNow - dob).TotalDays / 365.25);
                if (age < prefs.MinAge || age > prefs.MaxAge) continue;
            }

            var score = ComputeScore(prefs, questionnaire);
            results.Add(new RecommendationResponse(
                profile.UserId, profile.Bio, profile.DateOfBirth, score, profile.Badges));
        }

        return results.OrderByDescending(r => r.Score);
    }

    public async Task<MatchResponse?> Like(string userId, string targetId)
    {
        if (!_likes.ContainsKey(userId)) _likes[userId] = new();
        _likes[userId].Add(targetId);

        if (_likes.TryGetValue(targetId, out var targetLikes) && targetLikes.Contains(userId))
        {
            var match = new MatchResponse(Guid.NewGuid().ToString(), userId, targetId);
            _matches[match.MatchId] = match;
            return match;
        }

        return await Task.FromResult<MatchResponse?>(null);
    }

    public Task Pass(string userId, string targetId)
    {
        if (!_passes.ContainsKey(userId)) _passes[userId] = new();
        _passes[userId].Add(targetId);
        return Task.CompletedTask;
    }

    public Task Block(string userId, string targetId)
    {
        if (!_blocks.ContainsKey(userId)) _blocks[userId] = new();
        _blocks[userId].Add(targetId);
        return Task.CompletedTask;
    }

    private static int ComputeScore(SetPreferencesRequest? prefs, Dictionary<string, string>? questionnaire)
    {
        var score = 10;
        if (prefs?.Interests.Length > 0) score += 5;
        if (questionnaire != null) score += questionnaire.Count;
        return score;
    }
}
