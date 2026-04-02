import 'package:flutter/material.dart';

class PasswordRecoveryScreen extends StatelessWidget {
  const PasswordRecoveryScreen({super.key});

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(title: const Text('Recover password')),
      body: const Center(child: Text('Enter your email to reset your password')),
    );
  }
}
