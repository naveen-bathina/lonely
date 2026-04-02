using Lonely.Api.Discovery;

namespace Lonely.Api.Messaging;

public record SendMessageRequest(string SenderId, string Text);

public record MessageResponse(
    string MessageId,
    string MatchId,
    string SenderId,
    string Text,
    DateTime SentAt,
    DateTime? ReadAt);

public record ReadReceiptsSettingsRequest(string UserId, bool ReadReceiptsEnabled);

public record GhostingStatusResponse(bool IsIdle, double IdleHours);

public class MatchNotFoundException : Exception
{
    public MatchNotFoundException(string matchId) : base($"Match {matchId} not found.") { }
}

public interface IMessageService
{
    Task<MessageResponse> Send(string matchId, SendMessageRequest request);
    Task<IEnumerable<MessageResponse>> GetThread(string matchId, string? viewerId = null);
    Task SetReadReceipts(string matchId, ReadReceiptsSettingsRequest request);
    Task<GhostingStatusResponse> GetGhostingStatus(string matchId, string userId, int thresholdHours);
}

public class MessageService : IMessageService
{
    private readonly IDiscoveryService _discoveryService;
    private readonly Dictionary<string, List<StoredMessage>> _threads = new();
    private readonly Dictionary<string, HashSet<string>> _readReceiptsOff = new();

    private record StoredMessage(string MessageId, string MatchId, string SenderId, string Text, DateTime SentAt);

    public MessageService(IDiscoveryService discoveryService)
    {
        _discoveryService = discoveryService;
    }

    public async Task<MessageResponse> Send(string matchId, SendMessageRequest request)
    {
        if (!await _discoveryService.MatchExists(matchId))
            throw new MatchNotFoundException(matchId);

        if (!_threads.ContainsKey(matchId)) _threads[matchId] = new();

        var msg = new StoredMessage(Guid.NewGuid().ToString(), matchId, request.SenderId, request.Text, DateTime.UtcNow);
        _threads[matchId].Add(msg);
        return ToResponse(msg, viewerId: null);
    }

    public async Task<IEnumerable<MessageResponse>> GetThread(string matchId, string? viewerId = null)
    {
        if (!await _discoveryService.MatchExists(matchId))
            throw new MatchNotFoundException(matchId);

        if (!_threads.TryGetValue(matchId, out var messages))
            return Enumerable.Empty<MessageResponse>();

        return messages
            .OrderBy(m => m.SentAt)
            .Select(m => ToResponse(m, viewerId));
    }

    public Task SetReadReceipts(string matchId, ReadReceiptsSettingsRequest request)
    {
        if (!_readReceiptsOff.ContainsKey(matchId)) _readReceiptsOff[matchId] = new();
        if (request.ReadReceiptsEnabled)
            _readReceiptsOff[matchId].Remove(request.UserId);
        else
            _readReceiptsOff[matchId].Add(request.UserId);
        return Task.CompletedTask;
    }

    public async Task<GhostingStatusResponse> GetGhostingStatus(string matchId, string userId, int thresholdHours)
    {
        if (!await _discoveryService.MatchExists(matchId))
            throw new MatchNotFoundException(matchId);

        if (!_threads.TryGetValue(matchId, out var messages) || !messages.Any())
            return new GhostingStatusResponse(false, 0);

        var lastFromUser = messages.Where(m => m.SenderId == userId).MaxBy(m => m.SentAt);
        if (lastFromUser == null)
            return new GhostingStatusResponse(false, 0);

        var hasReply = messages.Any(m => m.SenderId != userId && m.SentAt > lastFromUser.SentAt);
        if (hasReply)
            return new GhostingStatusResponse(false, 0);

        var idleHours = (DateTime.UtcNow - lastFromUser.SentAt).TotalHours;
        return new GhostingStatusResponse(idleHours >= thresholdHours, idleHours);
    }

    private static MessageResponse ToResponse(StoredMessage msg, string? viewerId)
    {
        // readAt is null until a "mark as read" action is recorded.
        // In a full implementation, readAt would be populated by a separate endpoint,
        // and hidden from the response when the recipient has disabled read receipts.
        return new MessageResponse(msg.MessageId, msg.MatchId, msg.SenderId, msg.Text, msg.SentAt, ReadAt: null);
    }
}
