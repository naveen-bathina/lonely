import 'package:flutter/material.dart';
import 'auth_service.dart';
import 'otp_screen.dart';

class RegistrationScreen extends StatefulWidget {
  final AuthService authService;

  const RegistrationScreen({super.key, required this.authService});

  @override
  State<RegistrationScreen> createState() => _RegistrationScreenState();
}

class _RegistrationScreenState extends State<RegistrationScreen> {
  final _emailController = TextEditingController();
  final _passwordController = TextEditingController();
  final _dobController = TextEditingController();
  bool _gdprConsent = false;

  Future<void> _submit() async {
    final result = await widget.authService.register(RegisterRequest(
      email: _emailController.text,
      password: _passwordController.text,
      dateOfBirth: _dobController.text,
      gdprConsent: _gdprConsent,
    ));

    if (!mounted) return;
    Navigator.of(context).pushReplacement(
      MaterialPageRoute(builder: (_) => OtpScreen(userId: result.userId)),
    );
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(title: const Text('Create account')),
      body: Padding(
        padding: const EdgeInsets.all(16),
        child: Column(
          children: [
            TextField(
              key: const Key('email_field'),
              controller: _emailController,
              decoration: const InputDecoration(labelText: 'Email'),
            ),
            TextField(
              key: const Key('password_field'),
              controller: _passwordController,
              obscureText: true,
              decoration: const InputDecoration(labelText: 'Password'),
            ),
            TextField(
              key: const Key('date_of_birth_field'),
              controller: _dobController,
              decoration: const InputDecoration(labelText: 'Date of birth (YYYY-MM-DD)'),
            ),
            Row(
              children: [
                Checkbox(
                  key: const Key('gdpr_consent_checkbox'),
                  value: _gdprConsent,
                  onChanged: (v) => setState(() => _gdprConsent = v ?? false),
                ),
                const Expanded(child: Text('I agree to the Terms and Privacy Policy')),
              ],
            ),
            ElevatedButton(
              key: const Key('submit_button'),
              onPressed: _gdprConsent ? _submit : null,
              child: const Text('Create account'),
            ),
          ],
        ),
      ),
    );
  }
}
