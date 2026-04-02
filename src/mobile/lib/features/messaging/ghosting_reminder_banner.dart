import 'package:flutter/material.dart';

class GhostingReminderBanner extends StatelessWidget {
  final bool isIdle;

  const GhostingReminderBanner({super.key, required this.isIdle});

  @override
  Widget build(BuildContext context) {
    if (!isIdle) return const SizedBox.shrink();

    return Container(
      key: const Key('ghosting_banner'),
      width: double.infinity,
      padding: const EdgeInsets.all(12),
      color: Colors.amber.shade100,
      child: const Text(
        "👻 No reply yet — send a follow-up?",
        textAlign: TextAlign.center,
      ),
    );
  }
}
