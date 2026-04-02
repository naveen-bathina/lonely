import 'package:flutter/material.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:mobile/features/auth/registration_screen.dart';
import 'package:mobile/features/auth/otp_screen.dart';
import 'package:mobile/features/auth/auth_service.dart';

class _StubAuthService implements AuthService {
  @override
  Future<RegistrationResult> register(RegisterRequest request) async {
    return RegistrationResult(userId: 'user-123');
  }
}

void main() {
  testWidgets('Submit registration navigates to OTP screen', (WidgetTester tester) async {
    await tester.pumpWidget(MaterialApp(
      home: RegistrationScreen(authService: _StubAuthService()),
    ));

    await tester.enterText(find.byKey(const Key('email_field')), 'alice@example.com');
    await tester.enterText(find.byKey(const Key('password_field')), 'Password1!');
    await tester.enterText(find.byKey(const Key('date_of_birth_field')), '1990-01-01');
    await tester.tap(find.byKey(const Key('gdpr_consent_checkbox')));
    await tester.pump();
    await tester.tap(find.byKey(const Key('submit_button')));
    await tester.pumpAndSettle();

    expect(find.byType(OtpScreen), findsOneWidget);
  });

  testWidgets('Submit button is disabled when GDPR consent is not checked', (WidgetTester tester) async {
    await tester.pumpWidget(MaterialApp(
      home: RegistrationScreen(authService: _StubAuthService()),
    ));

    await tester.enterText(find.byKey(const Key('email_field')), 'alice@example.com');
    await tester.enterText(find.byKey(const Key('password_field')), 'Password1!');
    await tester.enterText(find.byKey(const Key('date_of_birth_field')), '1990-01-01');
    // gdpr_consent_checkbox intentionally NOT tapped

    final submitButton = tester.widget<ElevatedButton>(find.byKey(const Key('submit_button')));
    expect(submitButton.onPressed, isNull, reason: 'Submit must be disabled without GDPR consent');
  });
}
