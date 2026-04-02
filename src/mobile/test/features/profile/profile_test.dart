import 'package:flutter/material.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:mobile/features/profile/profile_service.dart';
import 'package:mobile/features/profile/profile_setup_screen.dart';
import 'package:mobile/features/profile/preferences_screen.dart';
import 'package:mobile/features/profile/profile_card.dart';

class _StubProfileService implements ProfileService {
  ProfileRequest? lastSaved;

  @override
  Future<void> saveProfile(ProfileRequest request) async {
    lastSaved = request;
  }

  @override
  Future<void> savePreferences(PreferencesRequest request) async {}
}

void main() {
  testWidgets('ProfileSetupScreen - save navigates to confirmation', (tester) async {
    final service = _StubProfileService();
    await tester.pumpWidget(MaterialApp(
      home: ProfileSetupScreen(profileService: service),
    ));

    await tester.enterText(find.byKey(const Key('bio_field')), 'Love hiking.');
    await tester.enterText(find.byKey(const Key('dob_field')), '1992-06-15');
    await tester.tap(find.byKey(const Key('save_button')));
    await tester.pumpAndSettle();

    expect(find.byKey(const Key('profile_saved_confirmation')), findsOneWidget);
  });

  testWidgets('PreferencesScreen - save calls service with correct values', (tester) async {
    final service = _StubProfileService();
    await tester.pumpWidget(MaterialApp(
      home: PreferencesScreen(profileService: service),
    ));

    await tester.drag(find.byKey(const Key('age_range_slider')), const Offset(20, 0));
    await tester.pump();
    await tester.tap(find.byKey(const Key('save_preferences_button')));
    await tester.pumpAndSettle();

    expect(find.byKey(const Key('preferences_saved_confirmation')), findsOneWidget);
  });

  testWidgets('ProfileCard - shows Verified and Active badges', (tester) async {
    await tester.pumpWidget(const MaterialApp(
      home: Scaffold(
        body: ProfileCard(
          name: 'Alice',
          age: 30,
          badges: ['Verified', 'Active'],
        ),
      ),
    ));

    expect(find.text('Verified'), findsOneWidget);
    expect(find.text('Active'), findsOneWidget);
  });

  testWidgets('ProfileCard - does not show badge when not present', (tester) async {
    await tester.pumpWidget(const MaterialApp(
      home: Scaffold(
        body: ProfileCard(
          name: 'Bob',
          age: 28,
          badges: ['New'],
        ),
      ),
    ));

    expect(find.text('Verified'), findsNothing);
  });
}
