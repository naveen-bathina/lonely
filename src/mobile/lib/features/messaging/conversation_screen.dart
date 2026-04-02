import 'package:flutter/material.dart';
import 'messaging_service.dart';
import 'message_bubble.dart';
import 'ghosting_reminder_banner.dart';

class ConversationScreen extends StatefulWidget {
  final String matchId;
  final String currentUserId;
  final MessagingService messagingService;

  const ConversationScreen({
    super.key,
    required this.matchId,
    required this.currentUserId,
    required this.messagingService,
  });

  @override
  State<ConversationScreen> createState() => _ConversationScreenState();
}

class _ConversationScreenState extends State<ConversationScreen> {
  List<ChatMessage> _messages = [];
  bool _isIdle = false;
  final _textController = TextEditingController();

  @override
  void initState() {
    super.initState();
    _load();
  }

  Future<void> _load() async {
    final messages = await widget.messagingService.getThread(widget.matchId);
    final ghosting = await widget.messagingService
        .getGhostingStatus(widget.matchId, widget.currentUserId);
    setState(() {
      _messages = messages;
      _isIdle = ghosting.isIdle;
    });
  }

  Future<void> _send() async {
    final text = _textController.text.trim();
    if (text.isEmpty) return;
    final msg = await widget.messagingService
        .sendMessage(widget.matchId, widget.currentUserId, text);
    _textController.clear();
    setState(() => _messages.add(msg));
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(title: const Text('Conversation')),
      body: Column(
        children: [
          GhostingReminderBanner(isIdle: _isIdle),
          Expanded(
            child: ListView(
              children: _messages
                  .map((m) => MessageBubble(
                        message: m,
                        currentUserId: widget.currentUserId,
                      ))
                  .toList(),
            ),
          ),
          Padding(
            padding: const EdgeInsets.all(8),
            child: Row(
              children: [
                Expanded(
                  child: TextField(
                    key: const Key('message_input'),
                    controller: _textController,
                  ),
                ),
                IconButton(
                  key: const Key('send_button'),
                  icon: const Icon(Icons.send),
                  onPressed: _send,
                ),
              ],
            ),
          ),
        ],
      ),
    );
  }
}
