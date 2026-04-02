import 'package:flutter/material.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:mobile/features/moderation/moderation_service.dart';
import 'package:mobile/features/moderation/report_dialog.dart';
import 'package:mobile/features/moderation/moderation_queue_screen.dart';
import 'package:mobile/features/moderation/report_history_screen.dart';

class _StubModerationService implements ModerationService {
  ModerationReport? lastReport;
  final List<ModerationReport> queue;
  final List<ModerationReport> history;

  _StubModerationService({this.queue = const [], this.history = const []});

  @override
  Future<ModerationReport> submitReport(String reporterId, String targetId, String reason) async {
    lastReport = ModerationReport(
      reportId: 'r1',
      reporterId: reporterId,
      targetId: targetId,
      reason: reason,
      status: 'pending',
    );
    return lastReport!;
  }

  @override
  Future<List<ModerationReport>> getQueue() async => queue;

  @override
  Future<List<ModerationReport>> getHistory(String userId) async => history;

  @override
  Future<ModerationReport> resolve(String reportId, String resolution) async {
    return ModerationReport(
      reportId: reportId,
      reporterId: '',
      targetId: '',
      reason: '',
      status: resolution,
    );
  }
}

void main() {
  testWidgets('ReportDialog - submits report with chosen reason', (tester) async {
    final service = _StubModerationService();

    await tester.pumpWidget(MaterialApp(
      home: Scaffold(
        body: ReportDialog(
          reporterId: 'user1',
          targetId: 'user2',
          moderationService: service,
        ),
      ),
    ));

    await tester.tap(find.byKey(const Key('reason_harassment')));
    await tester.pump();
    await tester.tap(find.byKey(const Key('submit_report_button')));
    await tester.pumpAndSettle();

    expect(service.lastReport, isNotNull);
    expect(service.lastReport!.reason, 'harassment');
  });

  testWidgets('ModerationQueueScreen - shows pending reports', (tester) async {
    final service = _StubModerationService(queue: [
      ModerationReport(
        reportId: 'r1',
        reporterId: 'u1',
        targetId: 'u2',
        reason: 'spam',
        status: 'pending',
      ),
      ModerationReport(
        reportId: 'r2',
        reporterId: 'u3',
        targetId: 'u4',
        reason: 'harassment',
        status: 'pending',
      ),
    ]);

    await tester.pumpWidget(MaterialApp(
      home: ModerationQueueScreen(moderationService: service),
    ));
    await tester.pump();

    expect(find.text('spam'), findsOneWidget);
    expect(find.text('harassment'), findsOneWidget);
    expect(find.byKey(const Key('approve_button_r1')), findsOneWidget);
    expect(find.byKey(const Key('remove_button_r1')), findsOneWidget);
  });

  testWidgets('ReportHistoryScreen - shows submitted reports with status chips', (tester) async {
    final service = _StubModerationService(history: [
      ModerationReport(
        reportId: 'r1',
        reporterId: 'u1',
        targetId: 'u2',
        reason: 'spam',
        status: 'pending',
      ),
      ModerationReport(
        reportId: 'r2',
        reporterId: 'u1',
        targetId: 'u3',
        reason: 'abuse',
        status: 'approved',
      ),
    ]);

    await tester.pumpWidget(MaterialApp(
      home: ReportHistoryScreen(userId: 'u1', moderationService: service),
    ));
    await tester.pump();

    expect(find.text('spam'), findsOneWidget);
    expect(find.text('abuse'), findsOneWidget);
    expect(find.text('pending'), findsOneWidget);
    expect(find.text('approved'), findsOneWidget);
  });
}
