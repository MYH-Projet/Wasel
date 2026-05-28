import 'package:geolocator/geolocator.dart';
import 'package:http/http.dart' as http;
import 'dart:convert';

// Note: reverse geocoding uses Nominatim (no API key needed), not a Wasel endpoint

class LocationService {
  // ── current position ───────────────────────────────────────────

  Future<Position?> getCurrentPosition() async {
    final permission = await _ensurePermission();
    if (!permission) return null;

    try {
      return await Geolocator.getCurrentPosition(
        locationSettings: const LocationSettings(
          accuracy: LocationAccuracy.high,
        ),
      );
    } catch (_) {
      return null;
    }
  }

  Future<bool> _ensurePermission() async {
    bool serviceEnabled = await Geolocator.isLocationServiceEnabled();
    if (!serviceEnabled) return false;

    LocationPermission permission = await Geolocator.checkPermission();
    if (permission == LocationPermission.denied) {
      permission = await Geolocator.requestPermission();
    }
    if (permission == LocationPermission.denied ||
        permission == LocationPermission.deniedForever) {
      return false;
    }
    return true;
  }

  // ── reverse geocoding ──────────────────────────────────────────

  Future<AddressResult?> reverseGeocode(double lat, double lng) async {
    try {
      final uri = Uri.parse(
        'https://nominatim.openstreetmap.org/reverse?lat=$lat&lon=$lng&format=json',
      );
      final response = await http.get(
        uri,
        headers: {'Accept-Language': 'en', 'User-Agent': 'wasel-app'},
      );
      if (response.statusCode != 200) return null;

      final data = jsonDecode(response.body);
      final address = data['address'] as Map<String, dynamic>;

      return AddressResult(
        label: data['display_name'] as String,
        street: _buildStreet(address),
        city:
            (address['city'] ?? address['town'] ?? address['village'] ?? '')
                as String,
        postalCode: (address['postcode'] ?? '') as String,
        country: (address['country'] ?? 'Morocco') as String,
        latitude: lat,
        longitude: lng,
      );
    } catch (_) {
      return null;
    }
  }

  String _buildStreet(Map<String, dynamic> address) {
    final road = address['road'] ?? '';
    final houseNumber = address['house_number'] ?? '';
    if (houseNumber.isNotEmpty) return '$houseNumber $road';
    return road as String;
  }
}

class AddressResult {
  final String label;
  final String street;
  final String city;
  final String postalCode;
  final String country;
  final double latitude;
  final double longitude;

  const AddressResult({
    required this.label,
    required this.street,
    required this.city,
    required this.postalCode,
    required this.country,
    required this.latitude,
    required this.longitude,
  });

  // convenience: converts to the shape POST /api/deliveries expects
  Map<String, dynamic> toJson() => {
    'label': label,
    'street': street,
    'city': city,
    'postalCode': postalCode,
    'country': country,
    'latitude': latitude,
    'longitude': longitude,
  };
}
