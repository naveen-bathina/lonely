import 'package:flutter/material.dart';
import 'discovery_service.dart';

class SwipeCard extends StatelessWidget {
  final DiscoveryProfile profile;
  final void Function(String userId) onLike;
  final void Function(String userId) onPass;

  const SwipeCard({
    super.key,
    required this.profile,
    required this.onLike,
    required this.onPass,
  });

  @override
  Widget build(BuildContext context) {
    return Card(
      margin: const EdgeInsets.all(12),
      child: Padding(
        padding: const EdgeInsets.all(16),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            Text(profile.name,
                style: Theme.of(context).textTheme.headlineSmall),
            Text('Age: ${profile.age}'),
            if (profile.badges.isNotEmpty)
              Wrap(
                spacing: 6,
                children: profile.badges
                    .map((b) => Chip(label: Text(b)))
                    .toList(),
              ),
            Row(
              mainAxisAlignment: MainAxisAlignment.spaceEvenly,
              children: [
                ElevatedButton(
                  key: const Key('pass_button'),
                  onPressed: () => onPass(profile.userId),
                  child: const Text('Pass'),
                ),
                ElevatedButton(
                  key: const Key('like_button'),
                  onPressed: () => onLike(profile.userId),
                  child: const Text('Like'),
                ),
              ],
            ),
          ],
        ),
      ),
    );
  }
}
