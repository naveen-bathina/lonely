class RegisterRequest {
  final String email;
  final String password;
  final String dateOfBirth;
  final bool gdprConsent;

  const RegisterRequest({
    required this.email,
    required this.password,
    required this.dateOfBirth,
    required this.gdprConsent,
  });
}

class RegistrationResult {
  final String userId;
  const RegistrationResult({required this.userId});
}

abstract class AuthService {
  Future<RegistrationResult> register(RegisterRequest request);
}

class LoginRequest {
  final String email;
  final String password;
  const LoginRequest({required this.email, required this.password});
}

class LoginResult {
  final String accessToken;
  final String refreshToken;
  const LoginResult({required this.accessToken, required this.refreshToken});
}

abstract class LoginService {
  Future<LoginResult> login(LoginRequest request);
}
