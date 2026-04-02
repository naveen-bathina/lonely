namespace Lonely.Api.Meetup;

public record ProposeMeetupRequest(string ProposerId, string MatchId);

public record MeetupProposalResponse(
    string ProposalId,
    string ProposerId,
    string MatchId,
    string State,
    DateTimeOffset CreatedAt,
    DateTimeOffset ExpiresAt);

public interface IMeetupService
{
    Task<MeetupProposalResponse> Propose(ProposeMeetupRequest request);
    Task<MeetupProposalResponse> GetProposal(string proposalId);
    Task<MeetupProposalResponse> Accept(string proposalId);
    Task<MeetupProposalResponse> Decline(string proposalId);
    Task<MeetupProposalResponse> Expire(string proposalId);
}

public class MeetupService : IMeetupService
{
    private readonly Dictionary<string, MeetupProposalResponse> _proposals = new();

    // proposalId → matchId index for duplicate-active-proposal check
    private readonly Dictionary<string, string> _activeByMatch = new();

    private static readonly TimeSpan DefaultTtl = TimeSpan.FromHours(48);

    public Task<MeetupProposalResponse> Propose(ProposeMeetupRequest request)
    {
        if (_activeByMatch.ContainsKey(request.MatchId))
            throw new InvalidOperationException("An active proposal already exists for this match.");

        var proposal = new MeetupProposalResponse(
            Guid.NewGuid().ToString(),
            request.ProposerId,
            request.MatchId,
            "pending",
            DateTimeOffset.UtcNow,
            DateTimeOffset.UtcNow.Add(DefaultTtl));

        _proposals[proposal.ProposalId] = proposal;
        _activeByMatch[request.MatchId] = proposal.ProposalId;
        return Task.FromResult(proposal);
    }

    public Task<MeetupProposalResponse> GetProposal(string proposalId)
    {
        if (!_proposals.TryGetValue(proposalId, out var proposal))
            throw new KeyNotFoundException($"Proposal {proposalId} not found.");
        return Task.FromResult(proposal);
    }

    public Task<MeetupProposalResponse> Accept(string proposalId) =>
        Transition(proposalId, "accepted");

    public Task<MeetupProposalResponse> Decline(string proposalId) =>
        Transition(proposalId, "declined");

    public Task<MeetupProposalResponse> Expire(string proposalId) =>
        Transition(proposalId, "expired");

    private Task<MeetupProposalResponse> Transition(string proposalId, string newState)
    {
        if (!_proposals.TryGetValue(proposalId, out var proposal))
            throw new KeyNotFoundException($"Proposal {proposalId} not found.");

        var updated = proposal with { State = newState };
        _proposals[proposalId] = updated;

        if (newState != "pending")
            _activeByMatch.Remove(proposal.MatchId);

        return Task.FromResult(updated);
    }
}
