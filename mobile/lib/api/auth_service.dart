import 'package:flutter_appauth/flutter_appauth.dart';
import 'package:flutter_secure_storage/flutter_secure_storage.dart';
import 'package:http/http.dart' as http;
import 'dart:convert';
import 'package:wasel/config.dart';

enum UserMode { client, driver }

class AuthService {
  String? _accessToken;
  String? _refreshToken;

  final _appAuth = FlutterAppAuth();
  final _storage = FlutterSecureStorage();

  // ── token persistence ──────────────────────────────────────────

  Future<String?> getAccessToken() async {
    _accessToken ??= await _storage.read(key: 'access-token');
    return _accessToken;
  }

  Future<String?> _getRefreshToken() async {
    _refreshToken ??= await _storage.read(key: 'refresh-token');
    return _refreshToken;
  }

  Future<void> _persistTokens(String access, String refresh) async {
    _accessToken = access;
    _refreshToken = refresh;
    await Future.wait([
      _storage.write(key: 'access-token', value: access),
      _storage.write(key: 'refresh-token', value: refresh),
    ]);
  }

  Future<void> clearTokens() async {
    _accessToken = null;
    _refreshToken = null;
    await Future.wait([
      _storage.delete(key: 'access-token'),
      _storage.delete(key: 'refresh-token'),
    ]);
  }

  // ── mode ───────────────────────────────────────────────────────

  Future<UserMode> getMode() async {
    final stored = await _storage.read(key: 'user-mode');
    return stored == 'driver' ? UserMode.driver : UserMode.client;
  }

  Future<void> setMode(UserMode mode) async {
    await _storage.write(
      key: 'user-mode',
      value: mode == UserMode.driver ? 'driver' : 'client',
    );
  }

  // ── auth check ─────────────────────────────────────────────────

  Future<bool> isAuthenticated() async {
    final token = await getAccessToken();
    if (token == null) return false;

    if (await _pingAuthMe(token)) return true;

    final refreshed = await _tryRefresh();
    if (!refreshed) {
      await clearTokens();
      return false;
    }

    final newToken = await getAccessToken();
    if (newToken != null && await _pingAuthMe(newToken)) return true;

    await clearTokens();
    return false;
  }

  Future<bool> _pingAuthMe(String token) async {
    try {
      final response = await http.get(
        Uri.parse('$API/api/auth/me'),
        headers: {'Authorization': 'Bearer $token'},
      );
      return response.statusCode == 200;
    } catch (_) {
      return false;
    }
  }

  Future<bool> _tryRefresh() async {
    final refreshToken = await _getRefreshToken();
    if (refreshToken == null) return false;

    try {
      final response = await http.post(
        Uri.parse('$API/auth/realms/wasel/protocol/openid-connect/token'),
        headers: {'Content-Type': 'application/x-www-form-urlencoded'},
        body: {
          'grant_type': 'refresh_token',
          'client_id': 'wasel-mobile',
          'refresh_token': refreshToken,
        },
      );

      if (response.statusCode != 200) return false;

      final data = jsonDecode(response.body);
      await _persistTokens(
        data['access_token'] as String,
        data['refresh_token'] as String,
      );
      return true;
    } catch (_) {
      return false;
    }
  }

  // ── login / register ───────────────────────────────────────────

  Future<void> login() async => _authActions(['login']);
  Future<void> register() async => _authActions(['create']);

  Future<void> _authActions(List<String> actions) async {
    final result = await _appAuth.authorizeAndExchangeCode(
      AuthorizationTokenRequest(
        'wasel-mobile',
        'com.example.wasel://oauthredirect',
        discoveryUrl: '$API/auth/realms/wasel/.well-known/openid-configuration',
        promptValues: actions,
        scopes: ['openid', 'profile', 'email'],
        allowInsecureConnections: true,
      ),
    );

    if (result.accessToken == null || result.refreshToken == null) {
      throw Exception('Incomplete token response from Keycloak');
    }

    await _persistTokens(result.accessToken!, result.refreshToken!);
  }
}
