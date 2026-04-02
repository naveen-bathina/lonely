import 'package:flutter/material.dart';
import 'profile_service.dart';

class PreferencesScreen extends StatefulWidget {
  final ProfileService profileService;
  const PreferencesScreen({super.key, required this.profileService});

  @override
  State<PreferencesScreen> createState() => _PreferencesScreenState();
}

class _PreferencesScreenState extends State<PreferencesScreen> {
  RangeValues _ageRange = const RangeValues(18, 45);
  bool _saved = false;

  Future<void> _save() async {
    await widget.profileService.savePreferences(PreferencesRequest(
      minAge: _ageRange.start.round(),
      maxAge: _ageRange.end.round(),
      maxDistanceKm: 50,
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
              Text('Preferences saved!', key: Key('preferences_saved_confirmation')),
            ],
          ),
        ),
      );
    }
    return Scaffold(
      appBar: AppBar(title: const Text('Your preferences')),
      body: Padding(
        padding: const EdgeInsets.all(16),
        child: Column(
          children: [
            const Text('Age range'),
            RangeSlider(
              key: const Key('age_range_slider'),
              values: _ageRange,
              min: 18,
              max: 80,
              divisions: 62,
              labels: RangeLabels(
                _ageRange.start.round().toString(),
                _ageRange.end.round().toString(),
              ),
              onChanged: (v) => setState(() => _ageRange = v),
            ),
            ElevatedButton(
              key: const Key('save_preferences_button'),
              onPressed: _save,
              child: const Text('Save preferences'),
            ),
          ],
        ),
      ),
    );
  }
}
