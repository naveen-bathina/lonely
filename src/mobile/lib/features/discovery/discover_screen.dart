import 'package:flutter/material.dart';
import 'discovery_service.dart';
import 'swipe_card.dart';
import 'match_banner.dart';

class DiscoverScreen extends StatefulWidget {
  final String userId;
  final DiscoveryService discoveryService;

  const DiscoverScreen({
    super.key,
    required this.userId,
    required this.discoveryService,
  });

  @override
  State<DiscoverScreen> createState() => _DiscoverScreenState();
}

class _DiscoverScreenState extends State<DiscoverScreen> {
  List<DiscoveryProfile> _profiles = [];
  MatchResult? _latestMatch;

  @override
  void initState() {
    super.initState();
    _load();
  }

  Future<void> _load() async {
    final profiles =
        await widget.discoveryService.getRecommendations(widget.userId);
    setState(() => _profiles = profiles);
  }

  Future<void> _onLike(String targetId) async {
    final match =
        await widget.discoveryService.like(widget.userId, targetId);
    if (match != null) setState(() => _latestMatch = match);
    setState(() => _profiles.removeWhere((p) => p.userId == targetId));
  }

  Future<void> _onPass(String targetId) async {
    await widget.discoveryService.pass(widget.userId, targetId);
    setState(() => _profiles.removeWhere((p) => p.userId == targetId));
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(title: const Text('Discover')),
      body: Column(
        children: [
          if (_latestMatch != null)
            MatchBanner(matchedName: _latestMatch!.user2Id),
          Expanded(
            child: ListView(
              children: _profiles
                  .map((p) => SwipeCard(
                        profile: p,
                        onLike: _onLike,
                        onPass: _onPass,
                      ))
                  .toList(),
            ),
          ),
        ],
      ),
    );
  }
}
