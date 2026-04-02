import 'package:flutter/material.dart';
import 'moderation_service.dart';

class ReportDialog extends StatefulWidget {
  final String reporterId;
  final String targetId;
  final ModerationService moderationService;

  const ReportDialog({
    super.key,
    required this.reporterId,
    required this.targetId,
    required this.moderationService,
  });

  @override
  State<ReportDialog> createState() => _ReportDialogState();
}

class _ReportDialogState extends State<ReportDialog> {
  String? _selectedReason;
  bool _submitted = false;

  static const _reasons = ['harassment', 'spam', 'nudity', 'fake', 'abuse'];

  Future<void> _submit() async {
    if (_selectedReason == null) return;
    await widget.moderationService.submitReport(
      widget.reporterId,
      widget.targetId,
      _selectedReason!,
    );
    setState(() => _submitted = true);
  }

  @override
  Widget build(BuildContext context) {
    if (_submitted) {
      return const Center(
        child: Text('Report submitted.', key: Key('report_submitted_confirmation')),
      );
    }
    return Column(
      mainAxisSize: MainAxisSize.min,
      children: [
        const Text('Report User', style: TextStyle(fontWeight: FontWeight.bold)),
        const SizedBox(height: 8),
        ..._reasons.map((r) => RadioListTile<String>(
              key: Key('reason_$r'),
              title: Text(r),
              value: r,
              groupValue: _selectedReason,
              onChanged: (v) => setState(() => _selectedReason = v),
            )),
        ElevatedButton(
          key: const Key('submit_report_button'),
          onPressed: _submit,
          child: const Text('Submit'),
        ),
      ],
    );
  }
}
