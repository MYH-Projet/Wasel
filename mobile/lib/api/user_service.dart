import 'package:http/http.dart' as http;
import 'dart:convert';
import 'package:wasel/config.dart';

import 'package:wasel/api/auth_service.dart';

class UserService {
  static Future<Map<String, dynamic>?> getUserInfo(
    AuthService authService,
  ) async {
    final token = await authService.getAccessToken();

    final response = await http.get(
      Uri.parse('$API/api/auth/me'),
      headers: {'Authorization': 'Bearer $token'},
    );
    if (response.statusCode == 200) {
      return jsonDecode(response.body);
    }
    return null;
  }
}
