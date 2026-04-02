import 'package:flutter/material.dart';
import 'moderation_service.dart';

class ReportHistoryScreen extends StatefulWidget {
  final String userId;
  final ModerationService moderationService;

  const ReportHistoryScreen({
    super.key,
    required this.userId,
    required this.moderationService,
  });

  @override
  State<ReportHistoryScreen> createState() => _ReportHistoryScreenState();
}

class _ReportHistoryScreenState extends State<ReportHistoryScreen> {
  List<ModerationReport> _history = [];

  @override
  void initState() {
    super.initState();
    _load();
  }

  Future<void> _load() async {
    final history = await widget.moderationService.getHistory(widget.userId);
    setState(() => _history = history);
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(title: const Text('My Reports')),
      body: ListView.builder(
        itemCount: _history.length,
        itemBuilder: (_, i) {
          final r = _history[i];
          return ListTile(
            title: Text(r.reason),
            subtitle: Text('Target: ${r.targetId}'),
            trailing: Chip(
              label: Text(r.status),
              backgroundColor: r.status == 'approved'
                  ? Colors.green.shade100
                  : r.status == 'removed'
                      ? Colors.red.shade100
                      : Colors.grey.shade200,
            ),
          );
        },
      ),
    );
  }
}
