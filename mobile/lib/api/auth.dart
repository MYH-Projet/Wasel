import 'package:flutter_appauth/flutter_appauth.dart';
import 'package:flutter_secure_storage/flutter_secure_storage.dart';

Future<void> registerUser() async {
  final conf = AuthorizationServiceConfiguration(
    authorizationEndpoint:
        'http://localhost:8000/auth/realms/wasel/protocol/openid-connect/auth',
    tokenEndpoint:
        'http://localhost:8000/auth/realms/wasel/protocol/openid-connect/token',
  );
}

Future<void> login() async {
  FlutterAppAuth appAuth = FlutterAppAuth();

  final conf = AuthorizationServiceConfiguration(
    authorizationEndpoint:
        'http://localhost:8000/auth/realms/wasel/protocol/openid-connect/auth',
    tokenEndpoint:
        'http://localhost:8000/auth/realms/wasel/protocol/openid-connect/token',
  );

  final AuthorizationTokenResponse result = await appAuth
      .authorizeAndExchangeCode(
        AuthorizationTokenRequest(
          // Refactor to variables
          'wasel-mobile',
          // no idea what to name the callback, this will do for now hh
          'com.example.wasel://oauthredirect',
          serviceConfiguration: conf,
          scopes: ['openid', 'profile', 'email'],
          // this shouldn't be used in prod
          allowInsecureConnections: true,
        ),
      );

  final accessToken = result.accessToken!;

  // Technically this is not needed since our mobile app belongs to the same app provider, so we don't need the idToken to
  // authenticate the user and register him in our user stores and we only need the access token to speak to the api
  final idToken = result.idToken!;

  // this could be null i'll need to verify it
  final refreshToken = result.refreshToken;

  if (refreshToken != null) storeSensitiveTokens(refreshToken);
}

Future<void> storeSensitiveTokens(String token) async {
  final storage = FlutterSecureStorage();

  await storage.write(key: 'refresh-token', value: token);
}
