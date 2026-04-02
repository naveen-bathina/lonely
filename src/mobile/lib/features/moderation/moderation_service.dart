class ModerationReport {
  final String reportId;
  final String reporterId;
  final String targetId;
  final String reason;
  final String status;

  const ModerationReport({
    required this.reportId,
    required this.reporterId,
    required this.targetId,
    required this.reason,
    required this.status,
  });
}

abstract class ModerationService {
  Future<ModerationReport> submitReport(
      String reporterId, String targetId, String reason);
  Future<List<ModerationReport>> getQueue();
  Future<List<ModerationReport>> getHistory(String userId);
  Future<ModerationReport> resolve(String reportId, String resolution);
}
