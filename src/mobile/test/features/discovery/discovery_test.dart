import 'package:flutter/material.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:mobile/features/discovery/discovery_service.dart';
import 'package:mobile/features/discovery/discover_screen.dart';
import 'package:mobile/features/discovery/swipe_card.dart';
import 'package:mobile/features/discovery/match_banner.dart';

class _StubDiscoveryService implements DiscoveryService {
  final List<DiscoveryProfile> profiles;
  String? likedUserId;
  String? passedUserId;

  _StubDiscoveryService({this.profiles = const []});

  @override
  Future<List<DiscoveryProfile>> getRecommendations(String userId) async =>
      profiles;

  @override
  Future<MatchResult?> like(String userId, String targetId) async {
    likedUserId = targetId;
    return null;
  }

  @override
  Future<void> pass(String userId, String targetId) async {
    passedUserId = targetId;
  }
}

void main() {
  testWidgets('DiscoverScreen - shows profile cards for each recommendation',
      (tester) async {
    final service = _StubDiscoveryService(profiles: [
      DiscoveryProfile(userId: 'u1', name: 'Alice', age: 28, badges: ['New']),
      DiscoveryProfile(userId: 'u2', name: 'Bob', age: 32, badges: []),
    ]);

    await tester.pumpWidget(MaterialApp(
      home: DiscoverScreen(userId: 'viewer', discoveryService: service),
    ));
    await tester.pump();

    expect(find.text('Alice'), findsOneWidget);
    expect(find.text('Bob'), findsOneWidget);
  });

  testWidgets('SwipeCard - onLike callback fires when like button tapped',
      (tester) async {
    String? likedId;
    await tester.pumpWidget(MaterialApp(
      home: Scaffold(
        body: SwipeCard(
          profile:
              DiscoveryProfile(userId: 'u1', name: 'Alice', age: 28, badges: []),
          onLike: (id) => likedId = id,
          onPass: (_) {},
        ),
      ),
    ));

    await tester.tap(find.byKey(const Key('like_button')));
    await tester.pump();

    expect(likedId, 'u1');
  });

  testWidgets('SwipeCard - onPass callback fires when pass button tapped',
      (tester) async {
    String? passedId;
    await tester.pumpWidget(MaterialApp(
      home: Scaffold(
        body: SwipeCard(
          profile:
              DiscoveryProfile(userId: 'u2', name: 'Bob', age: 30, badges: []),
          onLike: (_) {},
          onPass: (id) => passedId = id,
        ),
      ),
    ));

    await tester.tap(find.byKey(const Key('pass_button')));
    await tester.pump();

    expect(passedId, 'u2');
  });

  testWidgets('MatchBanner - displays match message and matched name',
      (tester) async {
    await tester.pumpWidget(const MaterialApp(
      home: Scaffold(
        body: MatchBanner(matchedName: 'Alice'),
      ),
    ));

    expect(find.text("It's a Match!"), findsOneWidget);
    expect(find.text('Alice'), findsOneWidget);
  });
}
