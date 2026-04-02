class ProfileRequest {
  final String bio;
  final String dateOfBirth;
  final List<String> datingGoals;

  const ProfileRequest({
    required this.bio,
    required this.dateOfBirth,
    required this.datingGoals,
  });
}

class PreferencesRequest {
  final int minAge;
  final int maxAge;
  final double maxDistanceKm;

  const PreferencesRequest({
    required this.minAge,
    required this.maxAge,
    required this.maxDistanceKm,
  });
}

abstract class ProfileService {
  Future<void> saveProfile(ProfileRequest request);
  Future<void> savePreferences(PreferencesRequest request);
}
