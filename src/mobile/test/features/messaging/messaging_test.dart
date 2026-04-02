import 'package:flutter/material.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:mobile/features/messaging/messaging_service.dart';
import 'package:mobile/features/messaging/conversation_screen.dart';
import 'package:mobile/features/messaging/message_bubble.dart';
import 'package:mobile/features/messaging/ghosting_reminder_banner.dart';

class _StubMessagingService implements MessagingService {
  final List<ChatMessage> messages;
  _StubMessagingService({this.messages = const []});

  @override
  Future<List<ChatMessage>> getThread(String matchId) async => messages;

  @override
  Future<ChatMessage> sendMessage(String matchId, String senderId, String text) async {
    return ChatMessage(
      messageId: 'new',
      senderId: senderId,
      text: text,
      sentAt: DateTime.now(),
      readAt: null,
    );
  }

  @override
  Future<void> setReadReceipts(String matchId, String userId, bool enabled) async {}

  @override
  Future<GhostingStatus> getGhostingStatus(String matchId, String userId) async {
    return const GhostingStatus(isIdle: false, idleHours: 0);
  }
}

void main() {
  testWidgets('ConversationScreen - renders messages from service', (tester) async {
    final service = _StubMessagingService(messages: [
      ChatMessage(
        messageId: 'm1',
        senderId: 'user1',
        text: 'Hello there!',
        sentAt: DateTime.now(),
        readAt: null,
      ),
      ChatMessage(
        messageId: 'm2',
        senderId: 'user2',
        text: 'Hey back!',
        sentAt: DateTime.now(),
        readAt: null,
      ),
    ]);

    await tester.pumpWidget(MaterialApp(
      home: ConversationScreen(
        matchId: 'match1',
        currentUserId: 'user1',
        messagingService: service,
      ),
    ));
    await tester.pump();

    expect(find.text('Hello there!'), findsOneWidget);
    expect(find.text('Hey back!'), findsOneWidget);
  });

  testWidgets('MessageBubble - own message aligned to right', (tester) async {
    await tester.pumpWidget(MaterialApp(
      home: Scaffold(
        body: MessageBubble(
          message: ChatMessage(
            messageId: 'm1',
            senderId: 'user1',
            text: 'Mine',
            sentAt: DateTime.now(),
            readAt: null,
          ),
          currentUserId: 'user1',
        ),
      ),
    ));

    final row = tester.widget<Row>(find.byType(Row).first);
    expect(row.mainAxisAlignment, MainAxisAlignment.end);
  });

  testWidgets('MessageBubble - other message aligned to left', (tester) async {
    await tester.pumpWidget(MaterialApp(
      home: Scaffold(
        body: MessageBubble(
          message: ChatMessage(
            messageId: 'm2',
            senderId: 'user2',
            text: 'Theirs',
            sentAt: DateTime.now(),
            readAt: null,
          ),
          currentUserId: 'user1',
        ),
      ),
    ));

    final row = tester.widget<Row>(find.byType(Row).first);
    expect(row.mainAxisAlignment, MainAxisAlignment.start);
  });

  testWidgets('GhostingReminderBanner - shown when isIdle', (tester) async {
    await tester.pumpWidget(const MaterialApp(
      home: Scaffold(
        body: GhostingReminderBanner(isIdle: true),
      ),
    ));

    expect(find.byKey(const Key('ghosting_banner')), findsOneWidget);
  });

  testWidgets('GhostingReminderBanner - hidden when not idle', (tester) async {
    await tester.pumpWidget(const MaterialApp(
      home: Scaffold(
        body: GhostingReminderBanner(isIdle: false),
      ),
    ));

    expect(find.byKey(const Key('ghosting_banner')), findsNothing);
  });
}
