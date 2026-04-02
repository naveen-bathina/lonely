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

public record PhotoUploadRequest(string FileName, string ContentType);

public record PhotoResponse(string PhotoId, string UserId, string Status);

public record PhotoStatusRequest(string Status);

public interface IProfileService
{
    Task<ProfileResponse> Upsert(string userId, UpsertProfileRequest request);
    Task<ProfileResponse?> Get(string userId);
    Task<IEnumerable<ProfileResponse>> GetDiscoverFeed();
    Task<PhotoResponse> UploadPhoto(string userId, PhotoUploadRequest request);
    Task<PhotoResponse> UpdatePhotoStatus(string userId, string photoId, string status);
    Task<IEnumerable<PhotoResponse>> GetPhotos(string userId, bool visibleOnly);
}

public class ProfileService : IProfileService
{
    private readonly Dictionary<string, ProfileResponse> _profiles = new();
    private readonly Dictionary<string, PhotoResponse> _photos = new();

    public Task<ProfileResponse> Upsert(string userId, UpsertProfileRequest request)
    {
        var isComplete = !string.IsNullOrWhiteSpace(request.Bio)
            && !string.IsNullOrWhiteSpace(request.DateOfBirth)
            && request.DatingGoals.Length > 0;

        var badges = ComputeBadges(userId);
        var profile = new ProfileResponse(userId, request.Bio, request.DateOfBirth, request.DatingGoals, isComplete, badges);
        _profiles[userId] = profile;
        return Task.FromResult(profile);
    }

    public Task<ProfileResponse?> Get(string userId)
    {
        _profiles.TryGetValue(userId, out var profile);
        return Task.FromResult(profile);
    }

    public Task<IEnumerable<ProfileResponse>> GetDiscoverFeed()
    {
        var feed = _profiles.Values.Where(p => p.IsComplete);
        return Task.FromResult(feed);
    }

    public Task<PhotoResponse> UploadPhoto(string userId, PhotoUploadRequest request)
    {
        var photoId = Guid.NewGuid().ToString();
        var photo = new PhotoResponse(photoId, userId, "pending");
        _photos[photoId] = photo;
        return Task.FromResult(photo);
    }

    public Task<PhotoResponse> UpdatePhotoStatus(string userId, string photoId, string status)
    {
        if (!_photos.TryGetValue(photoId, out var photo))
            throw new KeyNotFoundException($"Photo {photoId} not found.");

        var updated = photo with { Status = status };
        _photos[photoId] = updated;
        return Task.FromResult(updated);
    }

    public Task<IEnumerable<PhotoResponse>> GetPhotos(string userId, bool visibleOnly)
    {
        var photos = _photos.Values.Where(p => p.UserId == userId);
        if (visibleOnly)
            photos = photos.Where(p => p.Status == "approved");
        return Task.FromResult(photos);
    }

    private static string[] ComputeBadges(string userId) => new[] { "New" };
}
