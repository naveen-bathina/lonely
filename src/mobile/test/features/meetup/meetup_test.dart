import 'package:flutter/material.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:mobile/features/meetup/meetup_service.dart';
import 'package:mobile/features/meetup/meetup_proposal_card.dart';

class _StubMeetupService implements MeetupService {
  @override
  Future<MeetupProposal> propose(String proposerId, String matchId) async {
    return MeetupProposal(
      proposalId: 'p1',
      proposerId: proposerId,
      matchId: matchId,
      state: 'pending',
    );
  }

  @override
  Future<MeetupProposal> accept(String proposalId) async {
    return MeetupProposal(
      proposalId: proposalId,
      proposerId: 'user1',
      matchId: 'match1',
      state: 'accepted',
    );
  }

  @override
  Future<MeetupProposal> decline(String proposalId) async {
    return MeetupProposal(
      proposalId: proposalId,
      proposerId: 'user1',
      matchId: 'match1',
      state: 'declined',
    );
  }

  @override
  Future<MeetupProposal> getProposal(String proposalId) async {
    return MeetupProposal(
      proposalId: proposalId,
      proposerId: 'user1',
      matchId: 'match1',
      state: 'pending',
    );
  }
}

void main() {
  group('MeetupService', () {
    late MeetupService service;

    setUp(() {
      service = _StubMeetupService();
    });

    test('propose() returns proposal with pending state', () async {
      final proposal = await service.propose('user1', 'match1');

      expect(proposal.proposalId, isNotEmpty);
      expect(proposal.state, 'pending');
      expect(proposal.matchId, 'match1');
    });

    test('accept() returns proposal with accepted state', () async {
      final proposal = await service.accept('p1');

      expect(proposal.state, 'accepted');
    });

    test('decline() returns proposal with declined state', () async {
      final proposal = await service.decline('p1');

      expect(proposal.state, 'declined');
    });
  });

  group('MeetupProposalCard widget', () {
    testWidgets('renders pending state with accept and decline buttons',
        (tester) async {
      await tester.pumpWidget(MaterialApp(
        home: Scaffold(
          body: MeetupProposalCard(
            proposal: const MeetupProposal(
              proposalId: 'p1',
              proposerId: 'user2',
              matchId: 'match1',
              state: 'pending',
            ),
            currentUserId: 'user1',
            onAccept: () {},
            onDecline: () {},
          ),
        ),
      ));

      expect(find.text('Meetup Proposed'), findsOneWidget);
      expect(find.byKey(const Key('accept_proposal_btn')), findsOneWidget);
      expect(find.byKey(const Key('decline_proposal_btn')), findsOneWidget);
    });

    testWidgets('renders accepted state without action buttons', (tester) async {
      await tester.pumpWidget(MaterialApp(
        home: Scaffold(
          body: MeetupProposalCard(
            proposal: const MeetupProposal(
              proposalId: 'p1',
              proposerId: 'user2',
              matchId: 'match1',
              state: 'accepted',
            ),
            currentUserId: 'user1',
            onAccept: () {},
            onDecline: () {},
          ),
        ),
      ));

      expect(find.text('Meetup Accepted'), findsOneWidget);
      expect(find.byKey(const Key('accept_proposal_btn')), findsNothing);
    });
  });
}
