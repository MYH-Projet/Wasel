import 'package:http/http.dart' as http;
import 'dart:convert';
import 'package:wasel/api/auth_service.dart';
import 'package:wasel/api/location_service.dart';
import 'package:wasel/config.dart';

enum DeliveryServiceError { unauthorized, network, server }

class DeliveryResult {
  final Map<String, dynamic>? data;
  final DeliveryServiceError? error;

  const DeliveryResult.success(this.data) : error = null;
  const DeliveryResult.failure(this.error) : data = null;

  bool get isSuccess => error == null;
}

class DeliveryService {
  // ── create delivery ────────────────────────────────────────────

  static Future<DeliveryResult> createDelivery({
    required AuthService authService,
    required AddressResult pickupAddress,
    required AddressResult dropoffAddress,
    required double weight,
    required bool isFragile,
  }) async {
    final token = await authService.getAccessToken();
    if (token == null)
      return DeliveryResult.failure(DeliveryServiceError.unauthorized);

    try {
      final response = await http.post(
        Uri.parse('$API/api/deliveries'),
        headers: {
          'Authorization': 'Bearer $token',
          'Content-Type': 'application/json',
        },
        body: jsonEncode({
          'pickupAddress': pickupAddress.toJson(),
          'dropoffAddress': dropoffAddress.toJson(),
          'parcel': {'weight': weight, 'isFragile': isFragile},
          'paymentMethod': 1,
        }),
      );

      switch (response.statusCode) {
        case 200:
        case 201:
          return DeliveryResult.success(jsonDecode(response.body));
        case 401:
        case 403:
          return DeliveryResult.failure(DeliveryServiceError.unauthorized);
        default:
          return DeliveryResult.failure(DeliveryServiceError.server);
      }
    } catch (_) {
      return DeliveryResult.failure(DeliveryServiceError.network);
    }
  }

  // ── get my deliveries ──────────────────────────────────────────

  static Future<DeliveryResult> getMyDeliveries({
    required AuthService authService,
    int page = 1,
    int pageSize = 10,
  }) async {
    final token = await authService.getAccessToken();
    if (token == null)
      return DeliveryResult.failure(DeliveryServiceError.unauthorized);

    try {
      final response = await http.get(
        Uri.parse('$API/api/deliveries/my?page=$page&pageSize=$pageSize'),
        headers: {'Authorization': 'Bearer $token'},
      );

      switch (response.statusCode) {
        case 200:
          return DeliveryResult.success(jsonDecode(response.body));
        case 401:
        case 403:
          return DeliveryResult.failure(DeliveryServiceError.unauthorized);
        default:
          return DeliveryResult.failure(DeliveryServiceError.server);
      }
    } catch (_) {
      return DeliveryResult.failure(DeliveryServiceError.network);
    }
  }

  // ── get delivery by id ─────────────────────────────────────────

  static Future<DeliveryResult> getDelivery({
    required AuthService authService,
    required String id,
  }) async {
    final token = await authService.getAccessToken();
    if (token == null)
      return DeliveryResult.failure(DeliveryServiceError.unauthorized);

    try {
      final response = await http.get(
        Uri.parse('$API/api/deliveries/$id'),
        headers: {'Authorization': 'Bearer $token'},
      );

      switch (response.statusCode) {
        case 200:
          return DeliveryResult.success(jsonDecode(response.body));
        case 401:
        case 403:
          return DeliveryResult.failure(DeliveryServiceError.unauthorized);
        case 404:
          return DeliveryResult.failure(DeliveryServiceError.server);
        default:
          return DeliveryResult.failure(DeliveryServiceError.server);
      }
    } catch (_) {
      return DeliveryResult.failure(DeliveryServiceError.network);
    }
  }

  // ── cancel delivery ────────────────────────────────────────────

  static Future<DeliveryResult> cancelDelivery({
    required AuthService authService,
    required String id,
    String? reason,
  }) async {
    final token = await authService.getAccessToken();
    if (token == null)
      return DeliveryResult.failure(DeliveryServiceError.unauthorized);

    try {
      final response = await http.post(
        Uri.parse('$API/api/deliveries/$id/cancel'),
        headers: {
          'Authorization': 'Bearer $token',
          'Content-Type': 'application/json',
        },
        body: jsonEncode({'reason': reason ?? ''}),
      );

      switch (response.statusCode) {
        case 200:
          return DeliveryResult.success(jsonDecode(response.body));
        case 401:
        case 403:
          return DeliveryResult.failure(DeliveryServiceError.unauthorized);
        case 404:
          return DeliveryResult.failure(DeliveryServiceError.server);
        default:
          return DeliveryResult.failure(DeliveryServiceError.server);
      }
    } catch (_) {
      return DeliveryResult.failure(DeliveryServiceError.network);
    }
  }
}
