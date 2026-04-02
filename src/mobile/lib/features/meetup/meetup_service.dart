class MeetupProposal {
  final String proposalId;
  final String proposerId;
  final String matchId;
  final String state;

  const MeetupProposal({
    required this.proposalId,
    required this.proposerId,
    required this.matchId,
    required this.state,
  });
}

abstract class MeetupService {
  Future<MeetupProposal> propose(String proposerId, String matchId);
  Future<MeetupProposal> getProposal(String proposalId);
  Future<MeetupProposal> accept(String proposalId);
  Future<MeetupProposal> decline(String proposalId);
}
