import 'package:flutter/material.dart';
import 'profile_service.dart';

class ProfileSetupScreen extends StatefulWidget {
  final ProfileService profileService;
  const ProfileSetupScreen({super.key, required this.profileService});

  @override
  State<ProfileSetupScreen> createState() => _ProfileSetupScreenState();
}

class _ProfileSetupScreenState extends State<ProfileSetupScreen> {
  final _bioController = TextEditingController();
  final _dobController = TextEditingController();
  bool _saved = false;

  Future<void> _save() async {
    await widget.profileService.saveProfile(ProfileRequest(
      bio: _bioController.text,
      dateOfBirth: _dobController.text,
      datingGoals: const ['long-term'],
    ));
    setState(() => _saved = true);
  }

  @override
  Widget build(BuildContext context) {
    if (_saved) {
      return Scaffold(
        body: Center(
          child: Column(
            mainAxisAlignment: MainAxisAlignment.center,
            children: const [
              Icon(Icons.check_circle, color: Colors.green, size: 64),
              SizedBox(height: 16),
              Text('Profile saved!', key: Key('profile_saved_confirmation')),
            ],
          ),
        ),
      );
    }
    return Scaffold(
      appBar: AppBar(title: const Text('Set up profile')),
      body: Padding(
        padding: const EdgeInsets.all(16),
        child: Column(
          children: [
            TextField(
              key: const Key('bio_field'),
              controller: _bioController,
              decoration: const InputDecoration(labelText: 'About me'),
            ),
            TextField(
              key: const Key('dob_field'),
              controller: _dobController,
              decoration: const InputDecoration(labelText: 'Date of birth (YYYY-MM-DD)'),
            ),
            ElevatedButton(
              key: const Key('save_button'),
              onPressed: _save,
              child: const Text('Save profile'),
            ),
          ],
        ),
      ),
    );
  }
}
