import 'package:flutter/material.dart';

class MatchBanner extends StatelessWidget {
  final String matchedName;

  const MatchBanner({super.key, required this.matchedName});

  @override
  Widget build(BuildContext context) {
    return Container(
      width: double.infinity,
      padding: const EdgeInsets.all(16),
      color: Colors.pink.shade100,
      child: Column(
        children: [
          const Text(
            "It's a Match!",
            style: TextStyle(fontSize: 24, fontWeight: FontWeight.bold),
          ),
          Text(matchedName, style: const TextStyle(fontSize: 18)),
        ],
      ),
    );
  }
}
