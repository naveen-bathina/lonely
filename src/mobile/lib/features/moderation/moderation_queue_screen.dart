import 'package:flutter/material.dart';
import 'moderation_service.dart';

class ModerationQueueScreen extends StatefulWidget {
  final ModerationService moderationService;

  const ModerationQueueScreen({super.key, required this.moderationService});

  @override
  State<ModerationQueueScreen> createState() => _ModerationQueueScreenState();
}

class _ModerationQueueScreenState extends State<ModerationQueueScreen> {
  List<ModerationReport> _queue = [];

  @override
  void initState() {
    super.initState();
    _load();
  }

  Future<void> _load() async {
    final queue = await widget.moderationService.getQueue();
    setState(() => _queue = queue);
  }

  Future<void> _resolve(String reportId, String resolution) async {
    await widget.moderationService.resolve(reportId, resolution);
    await _load();
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(title: const Text('Moderation Queue')),
      body: ListView.builder(
        itemCount: _queue.length,
        itemBuilder: (_, i) {
          final r = _queue[i];
          return ListTile(
            title: Text(r.reason),
            subtitle: Text('Reported: ${r.targetId}'),
            trailing: Row(
              mainAxisSize: MainAxisSize.min,
              children: [
                IconButton(
                  key: Key('approve_button_${r.reportId}'),
                  icon: const Icon(Icons.check, color: Colors.green),
                  onPressed: () => _resolve(r.reportId, 'approved'),
                ),
                IconButton(
                  key: Key('remove_button_${r.reportId}'),
                  icon: const Icon(Icons.delete, color: Colors.red),
                  onPressed: () => _resolve(r.reportId, 'removed'),
                ),
              ],
            ),
          );
        },
      ),
    );
  }
}
