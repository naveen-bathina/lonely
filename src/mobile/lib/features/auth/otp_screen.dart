import 'package:flutter/material.dart';

class OtpScreen extends StatelessWidget {
  final String userId;

  const OtpScreen({super.key, required this.userId});

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(title: const Text('Verify your number')),
      body: const Center(child: Text('Enter the OTP sent to your email')),
    );
  }
}
