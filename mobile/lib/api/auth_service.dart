import 'package:flutter_appauth/flutter_appauth.dart';
import 'package:flutter_secure_storage/flutter_secure_storage.dart';
import 'package:wasel/config.dart';

class AuthService {
  String? _accessToken;
  String? _refreshToken;

  final appAuth = FlutterAppAuth();
  final storageService = FlutterSecureStorage();

  Future<bool> isAuthenticated() async {
    // Need to add refresh token logic later
    String? accessToken = await getAccessToken();
    if (accessToken == null) {
      return false;
    } else {
      return true;
    }
  }

  Future<String?> getAccessToken() async {
    _accessToken =
        _accessToken ?? await storageService.read(key: 'access-token');
    return _accessToken;
  }

  Future<void> login() async {
    await authActions(['login']);
  }

  Future<void> register() async {
    await authActions(['create']);
  }

  Future<void> authActions(List<String> actions) async {
    final clientId = 'wasel-mobile';
    final redirectUrl = 'com.example.wasel://oauthredirect';
    final discoveryUrl =
        '$API/auth/realms/wasel/.well-known/openid-configuration';

    final AuthorizationTokenResponse result = await appAuth
        .authorizeAndExchangeCode(
          AuthorizationTokenRequest(
            clientId,
            redirectUrl,
            discoveryUrl: discoveryUrl,
            promptValues: actions,
            scopes: ['openid', 'profile', 'email'],
            // this shouldn't be used in prod
            allowInsecureConnections: true,
          ),
        );

    _accessToken = result.accessToken!;

    _refreshToken = result.refreshToken!;

    await storageService.write(key: 'access-token', value: _accessToken);
    print('\nStored access token\n');

    storageService.write(key: 'refresh-token', value: _refreshToken);
    print('\nStored refresh token\n');
  }
}
