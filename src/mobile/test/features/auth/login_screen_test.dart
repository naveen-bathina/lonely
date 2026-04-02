import 'package:flutter/material.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:mobile/features/auth/login_screen.dart';
import 'package:mobile/features/auth/password_recovery_screen.dart';
import 'package:mobile/features/home/home_screen.dart';
import 'package:mobile/features/auth/auth_service.dart';

class _StubAuthService implements AuthService {
  @override
  Future<RegistrationResult> register(RegisterRequest request) async =>
      RegistrationResult(userId: 'user-123');
}

class _StubLoginService implements LoginService {
  @override
  Future<LoginResult> login(LoginRequest request) async =>
      LoginResult(accessToken: 'tok', refreshToken: 'ref');
}

void main() {
  testWidgets('Login with valid credentials navigates to HomeScreen', (WidgetTester tester) async {
    await tester.pumpWidget(MaterialApp(
      home: LoginScreen(loginService: _StubLoginService()),
    ));

    await tester.enterText(find.byKey(const Key('email_field')), 'alice@example.com');
    await tester.enterText(find.byKey(const Key('password_field')), 'Password1!');
    await tester.tap(find.byKey(const Key('login_button')));
    await tester.pumpAndSettle();

    expect(find.byType(HomeScreen), findsOneWidget);
  });

  testWidgets('Forgot password link navigates to PasswordRecoveryScreen', (WidgetTester tester) async {
    await tester.pumpWidget(MaterialApp(
      home: LoginScreen(loginService: _StubLoginService()),
    ));

    await tester.tap(find.byKey(const Key('forgot_password_link')));
    await tester.pumpAndSettle();

    expect(find.byType(PasswordRecoveryScreen), findsOneWidget);
  });
}
