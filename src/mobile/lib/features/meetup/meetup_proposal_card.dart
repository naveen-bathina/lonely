import 'package:flutter/material.dart';
import 'meetup_service.dart';

class MeetupProposalCard extends StatelessWidget {
  final MeetupProposal proposal;
  final String currentUserId;
  final VoidCallback onAccept;
  final VoidCallback onDecline;

  const MeetupProposalCard({
    super.key,
    required this.proposal,
    required this.currentUserId,
    required this.onAccept,
    required this.onDecline,
  });

  @override
  Widget build(BuildContext context) {
    final isPending = proposal.state == 'pending';
    final isProposer = proposal.proposerId == currentUserId;

    String label;
    switch (proposal.state) {
      case 'accepted':
        label = 'Meetup Accepted';
        break;
      case 'declined':
        label = 'Meetup Declined';
        break;
      case 'expired':
        label = 'Meetup Expired';
        break;
      default:
        label = 'Meetup Proposed';
    }

    return Card(
      child: Padding(
        padding: const EdgeInsets.all(12),
        child: Column(
          mainAxisSize: MainAxisSize.min,
          children: [
            Text(label, style: const TextStyle(fontWeight: FontWeight.bold)),
            if (isPending && !isProposer) ...[
              const SizedBox(height: 8),
              Row(
                mainAxisAlignment: MainAxisAlignment.spaceEvenly,
                children: [
                  ElevatedButton(
                    key: const Key('accept_proposal_btn'),
                    onPressed: onAccept,
                    child: const Text('Accept'),
                  ),
                  OutlinedButton(
                    key: const Key('decline_proposal_btn'),
                    onPressed: onDecline,
                    child: const Text('Decline'),
                  ),
                ],
              ),
            ],
          ],
        ),
      ),
    );
  }
}
