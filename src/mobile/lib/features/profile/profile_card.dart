import 'package:flutter/material.dart';

class ProfileCard extends StatelessWidget {
  final String name;
  final int age;
  final List<String> badges;

  const ProfileCard({
    super.key,
    required this.name,
    required this.age,
    required this.badges,
  });

  @override
  Widget build(BuildContext context) {
    return Card(
      child: Padding(
        padding: const EdgeInsets.all(12),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            Text('$name, $age', style: Theme.of(context).textTheme.titleMedium),
            const SizedBox(height: 8),
            Wrap(
              spacing: 6,
              children: badges
                  .map((b) => Chip(
                        label: Text(b),
                        backgroundColor: _badgeColor(b),
                      ))
                  .toList(),
            ),
          ],
        ),
      ),
    );
  }

  Color _badgeColor(String badge) => switch (badge) {
        'Verified' => Colors.blue.shade100,
        'Active' => Colors.green.shade100,
        'New' => Colors.orange.shade100,
        _ => Colors.grey.shade200,
      };
}
