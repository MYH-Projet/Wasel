import 'package:flutter/material.dart';
import 'package:flutter_map/flutter_map.dart';
import 'package:geolocator/geolocator.dart';
import 'package:latlong2/latlong.dart';
import 'package:wasel/api/delivery_service.dart';
import 'package:wasel/api/location_service.dart';
import 'package:wasel/main.dart';
import 'package:wasel/screens/client/specific_request_screen.dart';
import 'package:wasel/themes/colors.dart';
import 'package:wasel/themes/text_styles.dart';

enum _PickMode { none, pickup, dropoff }

class ClientHomeScreen extends StatefulWidget {
  const ClientHomeScreen({super.key});

  @override
  State<ClientHomeScreen> createState() => _ClientHomeScreenState();
}

class _ClientHomeScreenState extends State<ClientHomeScreen> {
  // ── services ───────────────────────────────────────────────────
  final _locationService = LocationService();
  final _mapController = MapController();

  // ── map state ──────────────────────────────────────────────────
  LatLng _mapCenter = const LatLng(33.5731, -7.5898); // Casablanca default
  _PickMode _pickMode = _PickMode.none;
  bool _isReverseGeocoding = false;

  // ── address state ──────────────────────────────────────────────
  AddressResult? _pickupAddress;
  AddressResult? _dropoffAddress;

  // ── form state ─────────────────────────────────────────────────
  final _weightController = TextEditingController();
  bool _isFragile = false;
  bool _isSubmitting = false;

  // ── sheet controller ───────────────────────────────────────────
  final _sheetController = DraggableScrollableController();

  @override
  void dispose() {
    _weightController.dispose();
    _sheetController.dispose();
    super.dispose();
  }

  // ── location helpers ───────────────────────────────────────────

  Future<AddressResult?> _getCurrentLocationAddress() async {
    final position = await _locationService.getCurrentPosition();
    if (position == null) {
      if (mounted) {
        ScaffoldMessenger.of(context).showSnackBar(
          const SnackBar(content: Text('Could not get current location')),
        );
      }
      return null;
    }
    final address = await _locationService.reverseGeocode(
      position.latitude,
      position.longitude,
    );
    if (address == null && mounted) {
      ScaffoldMessenger.of(context).showSnackBar(
        const SnackBar(content: Text('Could not resolve address')),
      );
    }
    return address;
  }

  Future<void> _centerOnCurrentLocation() async {
    final position = await _locationService.getCurrentPosition();
    if (position == null) return;
    _mapController.move(LatLng(position.latitude, position.longitude), 15);
  }

  // ── address picker bottom sheet ────────────────────────────────

  void _showAddressPicker(_PickMode mode) {
    showModalBottomSheet(
      context: context,
      shape: const RoundedRectangleBorder(
        borderRadius: BorderRadius.vertical(top: Radius.circular(16)),
      ),
      builder: (context) => Padding(
        padding: const EdgeInsets.all(24),
        child: Column(
          mainAxisSize: MainAxisSize.min,
          crossAxisAlignment: CrossAxisAlignment.stretch,
          children: [
            Text(
              mode == _PickMode.pickup ? 'Pickup address' : 'Dropoff address',
              style: headingText,
            ),
            const SizedBox(height: 24),
            OutlinedButton.icon(
              onPressed: () async {
                Navigator.pop(context);
                final address = await _getCurrentLocationAddress();
                if (address == null) return;
                setState(() {
                  if (mode == _PickMode.pickup) {
                    _pickupAddress = address;
                  } else {
                    _dropoffAddress = address;
                  }
                });
              },
              icon: const Icon(Icons.my_location_rounded),
              label: const Text('Your current location'),
              style: OutlinedButton.styleFrom(
                padding: const EdgeInsets.symmetric(vertical: 14),
                side: BorderSide(color: surfaceVariant),
                shape: RoundedRectangleBorder(
                  borderRadius: BorderRadius.circular(10),
                ),
              ),
            ),
            const SizedBox(height: 12),
            OutlinedButton.icon(
              onPressed: () {
                Navigator.pop(context);
                setState(() => _pickMode = mode);
                _sheetController.animateTo(
                  0.15,
                  duration: const Duration(milliseconds: 300),
                  curve: Curves.easeOut,
                );
              },
              icon: const Icon(Icons.map_rounded),
              label: const Text('Select on map'),
              style: OutlinedButton.styleFrom(
                padding: const EdgeInsets.symmetric(vertical: 14),
                side: BorderSide(color: surfaceVariant),
                shape: RoundedRectangleBorder(
                  borderRadius: BorderRadius.circular(10),
                ),
              ),
            ),
            const SizedBox(height: 24),
          ],
        ),
      ),
    );
  }

  // ── confirm map pin ────────────────────────────────────────────

  Future<void> _confirmMapPin() async {
    setState(() => _isReverseGeocoding = true);
    final address = await _locationService.reverseGeocode(
      _mapCenter.latitude,
      _mapCenter.longitude,
    );
    setState(() => _isReverseGeocoding = false);

    if (address == null) {
      if (mounted) {
        ScaffoldMessenger.of(context).showSnackBar(
          const SnackBar(content: Text('Could not resolve address')),
        );
      }
      return;
    }

    setState(() {
      if (_pickMode == _PickMode.pickup) {
        _pickupAddress = address;
      } else {
        _dropoffAddress = address;
      }
      _pickMode = _PickMode.none;
    });

    _sheetController.animateTo(
      0.5,
      duration: const Duration(milliseconds: 300),
      curve: Curves.easeOut,
    );
  }

  // ── submit delivery ────────────────────────────────────────────

  Future<void> _submitDelivery() async {
    if (_pickupAddress == null || _dropoffAddress == null) {
      ScaffoldMessenger.of(context).showSnackBar(
        const SnackBar(
          content: Text('Please set pickup and dropoff addresses'),
        ),
      );
      return;
    }

    final weightText = _weightController.text.trim();
    if (weightText.isEmpty) {
      ScaffoldMessenger.of(context).showSnackBar(
        const SnackBar(content: Text('Please enter parcel weight')),
      );
      return;
    }

    final weight = double.tryParse(weightText);
    if (weight == null || weight <= 0) {
      ScaffoldMessenger.of(context).showSnackBar(
        const SnackBar(content: Text('Please enter a valid weight')),
      );
      return;
    }

    setState(() => _isSubmitting = true);

    final authService = InheritedAuth.of(context).authService;
    final result = await DeliveryService.createDelivery(
      authService: authService,
      pickupAddress: _pickupAddress!,
      dropoffAddress: _dropoffAddress!,
      weight: weight,
      isFragile: _isFragile,
    );

    if (!mounted) return;
    setState(() => _isSubmitting = false);

    if (result.isSuccess) {
      final deliveryId = result.data!['id'] as String;
      Navigator.push(
        context,
        MaterialPageRoute(
          builder: (context) => SpecificRequestScreen(deliveryId: deliveryId),
        ),
      );
    } else {
      final message = switch (result.error) {
        DeliveryServiceError.unauthorized =>
          'Session expired, please sign in again',
        DeliveryServiceError.network => 'Could not reach the server',
        DeliveryServiceError.server => 'Something went wrong, please try again',
        null => 'Something went wrong',
      };
      ScaffoldMessenger.of(
        context,
      ).showSnackBar(SnackBar(content: Text(message)));
    }
  }

  // ── build ──────────────────────────────────────────────────────

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      body: Stack(
        children: [
          // ── map ──────────────────────────────────────────────
          FlutterMap(
            mapController: _mapController,
            options: MapOptions(
              initialCenter: _mapCenter,
              initialZoom: 13,
              onPositionChanged: (position, hasGesture) {
                if (hasGesture && _pickMode != _PickMode.none) {
                  setState(() => _mapCenter = position.center);
                }
              },
            ),
            children: [
              TileLayer(
                urlTemplate: 'https://tile.openstreetmap.org/{z}/{x}/{y}.png',
                userAgentPackageName: 'com.example.wasel',
              ),
              MarkerLayer(
                markers: [
                  if (_pickupAddress != null)
                    Marker(
                      point: LatLng(
                        _pickupAddress!.latitude,
                        _pickupAddress!.longitude,
                      ),
                      child: const Icon(
                        Icons.circle,
                        color: primaryColor,
                        size: 16,
                      ),
                    ),
                  if (_dropoffAddress != null)
                    Marker(
                      point: LatLng(
                        _dropoffAddress!.latitude,
                        _dropoffAddress!.longitude,
                      ),
                      child: const Icon(
                        Icons.location_on_rounded,
                        color: Colors.red,
                        size: 32,
                      ),
                    ),
                ],
              ),
            ],
          ),

          // ── center pin (pick mode) ────────────────────────────
          if (_pickMode != _PickMode.none)
            const Center(
              child: Icon(
                Icons.location_on_rounded,
                color: secondaryColor,
                size: 40,
              ),
            ),

          // ── my location button ────────────────────────────────
          Positioned(
            right: 16,
            bottom: MediaQuery.of(context).size.height * 0.5,
            child: FloatingActionButton.small(
              onPressed: _centerOnCurrentLocation,
              backgroundColor: Colors.white,
              foregroundColor: secondaryColor,
              elevation: 2,
              child: const Icon(Icons.my_location_rounded),
            ),
          ),

          // ── pick mode confirm bar ─────────────────────────────
          if (_pickMode != _PickMode.none)
            Positioned(
              bottom: MediaQuery.of(context).size.height * 0.18,
              left: 24,
              right: 24,
              child: ElevatedButton(
                onPressed: _isReverseGeocoding ? null : _confirmMapPin,
                style: ElevatedButton.styleFrom(
                  backgroundColor: secondaryColor,
                  foregroundColor: Colors.white,
                  padding: const EdgeInsets.symmetric(vertical: 14),
                  shape: RoundedRectangleBorder(
                    borderRadius: BorderRadius.circular(10),
                  ),
                ),
                child: _isReverseGeocoding
                    ? const SizedBox(
                        height: 20,
                        width: 20,
                        child: CircularProgressIndicator(
                          strokeWidth: 2,
                          color: Colors.white,
                        ),
                      )
                    : Text(
                        _pickMode == _PickMode.pickup
                            ? 'Set pickup here'
                            : 'Set dropoff here',
                        style: bolderLabelText.copyWith(color: Colors.white),
                      ),
              ),
            ),

          // ── bottom sheet ──────────────────────────────────────
          DraggableScrollableSheet(
            controller: _sheetController,
            initialChildSize: 0.5,
            minChildSize: 0.15,
            maxChildSize: 0.92,
            snap: true,
            snapSizes: const [0.15, 0.5, 0.92],
            builder: (context, scrollController) {
              return Container(
                decoration: const BoxDecoration(
                  color: Colors.white,
                  borderRadius: BorderRadius.vertical(top: Radius.circular(20)),
                  boxShadow: [
                    BoxShadow(
                      color: Colors.black12,
                      blurRadius: 10,
                      offset: Offset(0, -2),
                    ),
                  ],
                ),
                child: ListView(
                  controller: scrollController,
                  padding: const EdgeInsets.symmetric(horizontal: 24),
                  children: [
                    // drag handle
                    Center(
                      child: Container(
                        margin: const EdgeInsets.symmetric(vertical: 12),
                        width: 40,
                        height: 4,
                        decoration: BoxDecoration(
                          color: surfaceVariant,
                          borderRadius: BorderRadius.circular(2),
                        ),
                      ),
                    ),

                    Text('New delivery', style: headingText),
                    const SizedBox(height: 20),

                    // ── pickup field ────────────────────────────
                    _AddressField(
                      label: 'Pickup',
                      address: _pickupAddress,
                      icon: Icons.circle,
                      iconColor: primaryColor,
                      onTap: () => _showAddressPicker(_PickMode.pickup),
                    ),
                    const SizedBox(height: 4),
                    const Padding(
                      padding: EdgeInsets.only(left: 12),
                      child: SizedBox(
                        height: 16,
                        child: VerticalDivider(width: 1, color: Colors.black26),
                      ),
                    ),
                    const SizedBox(height: 4),

                    // ── dropoff field ───────────────────────────
                    _AddressField(
                      label: 'Where to?',
                      address: _dropoffAddress,
                      icon: Icons.location_on_rounded,
                      iconColor: Colors.red,
                      onTap: () => _showAddressPicker(_PickMode.dropoff),
                    ),

                    const SizedBox(height: 24),
                    Divider(color: surfaceVariant),
                    const SizedBox(height: 16),

                    // ── parcel section ──────────────────────────
                    Text('Parcel', style: subHeadingText),
                    const SizedBox(height: 16),

                    TextField(
                      controller: _weightController,
                      keyboardType: const TextInputType.numberWithOptions(
                        decimal: true,
                      ),
                      decoration: InputDecoration(
                        labelText: 'Weight (kg)',
                        border: OutlineInputBorder(
                          borderRadius: BorderRadius.circular(10),
                        ),
                        suffixText: 'kg',
                      ),
                    ),
                    const SizedBox(height: 12),

                    Row(
                      mainAxisAlignment: MainAxisAlignment.spaceBetween,
                      children: [
                        Text('Fragile', style: labelText),
                        Switch(
                          value: _isFragile,
                          onChanged: (val) => setState(() => _isFragile = val),
                          thumbColor: WidgetStatePropertyAll(primaryColor),
                          trackColor: WidgetStateProperty.resolveWith(
                            (states) => states.contains(WidgetState.selected)
                                ? primaryColor.withValues(alpha: 0.5)
                                : surfaceVariant,
                          ),
                        ),
                      ],
                    ),

                    const SizedBox(height: 16),
                    Divider(color: surfaceVariant),
                    const SizedBox(height: 16),

                    // ── price placeholder ───────────────────────
                    // TODO: replace with estimate API call once endpoint exists
                    Row(
                      mainAxisAlignment: MainAxisAlignment.spaceBetween,
                      children: [
                        Text('Est. Price', style: labelText),
                        Text('-- DH', style: headingText),
                      ],
                    ),

                    const SizedBox(height: 24),

                    // ── confirm button ──────────────────────────
                    ElevatedButton(
                      onPressed: _isSubmitting ? null : _submitDelivery,
                      style: ElevatedButton.styleFrom(
                        backgroundColor: primaryColor,
                        foregroundColor: onPrimary,
                        padding: const EdgeInsets.symmetric(vertical: 16),
                        shape: RoundedRectangleBorder(
                          borderRadius: BorderRadius.circular(10),
                        ),
                      ),
                      child: _isSubmitting
                          ? const SizedBox(
                              height: 20,
                              width: 20,
                              child: CircularProgressIndicator(
                                strokeWidth: 2,
                                color: Colors.white,
                              ),
                            )
                          : Text('Confirm Delivery', style: bolderLabelText),
                    ),
                    const SizedBox(height: 32),
                  ],
                ),
              );
            },
          ),
        ],
      ),
    );
  }
}

// ── address field widget ───────────────────────────────────────────

class _AddressField extends StatelessWidget {
  final String label;
  final AddressResult? address;
  final IconData icon;
  final Color iconColor;
  final VoidCallback onTap;

  const _AddressField({
    required this.label,
    required this.address,
    required this.icon,
    required this.iconColor,
    required this.onTap,
  });

  @override
  Widget build(BuildContext context) {
    return GestureDetector(
      onTap: onTap,
      child: Container(
        padding: const EdgeInsets.symmetric(horizontal: 12, vertical: 14),
        decoration: BoxDecoration(
          color: surfaceColor,
          borderRadius: BorderRadius.circular(10),
        ),
        child: Row(
          children: [
            Icon(icon, color: iconColor, size: 18),
            const SizedBox(width: 12),
            Expanded(
              child: Text(
                address?.label ?? label,
                style: bodyText.copyWith(
                  color: address == null ? Colors.black38 : onSurface,
                ),
                maxLines: 1,
                overflow: TextOverflow.ellipsis,
              ),
            ),
          ],
        ),
      ),
    );
  }
}
