class ChatMessage {
  final String messageId;
  final String senderId;
  final String text;
  final DateTime sentAt;
  final DateTime? readAt;

  const ChatMessage({
    required this.messageId,
    required this.senderId,
    required this.text,
    required this.sentAt,
    required this.readAt,
  });
}

class GhostingStatus {
  final bool isIdle;
  final double idleHours;

  const GhostingStatus({required this.isIdle, required this.idleHours});
}

abstract class MessagingService {
  Future<List<ChatMessage>> getThread(String matchId);
  Future<ChatMessage> sendMessage(String matchId, String senderId, String text);
  Future<void> setReadReceipts(String matchId, String userId, bool enabled);
  Future<GhostingStatus> getGhostingStatus(String matchId, String userId);
}
