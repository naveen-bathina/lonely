class DiscoveryProfile {
  final String userId;
  final String name;
  final int age;
  final List<String> badges;

  const DiscoveryProfile({
    required this.userId,
    required this.name,
    required this.age,
    required this.badges,
  });
}

class MatchResult {
  final String matchId;
  final String user1Id;
  final String user2Id;

  const MatchResult({
    required this.matchId,
    required this.user1Id,
    required this.user2Id,
  });
}

abstract class DiscoveryService {
  Future<List<DiscoveryProfile>> getRecommendations(String userId);
  Future<MatchResult?> like(String userId, String targetId);
  Future<void> pass(String userId, String targetId);
}
