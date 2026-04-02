import 'package:flutter/material.dart';
import 'auth_service.dart';
import 'password_recovery_screen.dart';
import '../home/home_screen.dart';

class LoginScreen extends StatefulWidget {
  final LoginService loginService;

  const LoginScreen({super.key, required this.loginService});

  @override
  State<LoginScreen> createState() => _LoginScreenState();
}

class _LoginScreenState extends State<LoginScreen> {
  final _emailController = TextEditingController();
  final _passwordController = TextEditingController();

  Future<void> _login() async {
    await widget.loginService.login(LoginRequest(
      email: _emailController.text,
      password: _passwordController.text,
    ));

    if (!mounted) return;
    Navigator.of(context).pushReplacement(
      MaterialPageRoute(builder: (_) => const HomeScreen()),
    );
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(title: const Text('Sign in')),
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
            ElevatedButton(
              key: const Key('login_button'),
              onPressed: _login,
              child: const Text('Sign in'),
            ),
            TextButton(
              key: const Key('forgot_password_link'),
              onPressed: () => Navigator.of(context).push(
                MaterialPageRoute(builder: (_) => const PasswordRecoveryScreen()),
              ),
              child: const Text('Forgot password?'),
            ),
          ],
        ),
      ),
    );
  }
}
