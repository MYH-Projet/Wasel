import 'package:flutter_secure_storage/flutter_secure_storage.dart';
import 'package:http/http.dart' as http;
import 'dart:convert';

Future<Object> getUserInfo() async {
  Uri uri = Uri.http('locahost:8000', '/api/auth/me');
  var accessToken = await FlutterSecureStorage().read(key: 'access-token');
  final res = await http.get(
    uri,
    headers: {'Authorization': 'Bearer $accessToken'},
  );
  if (res.statusCode == 200) {
    final userInfo = json.decode(res.body);

    print('');
    userInfo.forEach((key, value) {
      print('$key: $value');
    });
    print('');
    return userInfo;
  } else {
    throw Exception(res.body);
  }
}
