import 'dart:async';
import 'package:flutter/material.dart';
import 'package:flutter_map/flutter_map.dart';
import 'package:latlong2/latlong.dart';
import 'package:wasel/api/delivery_service.dart';
import 'package:wasel/main.dart';
import 'package:wasel/themes/colors.dart';
import 'package:wasel/themes/text_styles.dart';

// ── status helpers ─────────────────────────────────────────────────

const _terminalStatuses = {
  'DELIVERED',
  'CANCELLED_BY_CLIENT',
  'CANCELLED_BY_DRIVER',
  'CANCELLED_BY_ADMIN',
  'PROBLEM_REPORTED',
};

const _cancellableStatuses = {'CREATED', 'WAITING_DRIVER', 'ASSIGNED'};

const _activeStatuses = {
  'ACCEPTED',
  'ARRIVED_AT_PICKUP',
  'PICKED_UP',
  'IN_TRANSIT',
  'ARRIVED_AT_DROPOFF',
};

String _labelFor(String status) {
  return switch (status) {
    'CREATED' => 'Created',
    'WAITING_DRIVER' => 'Looking for a driver',
    'ASSIGNED' => 'Driver assigned',
    'ACCEPTED' => 'Driver accepted',
    'ARRIVED_AT_PICKUP' => 'Driver at pickup',
    'PICKED_UP' => 'Parcel picked up',
    'IN_TRANSIT' => 'On the way',
    'ARRIVED_AT_DROPOFF' => 'Driver at dropoff',
    'DELIVERED' => 'Delivered',
    'CANCELLED_BY_CLIENT' => 'Cancelled by you',
    'CANCELLED_BY_DRIVER' => 'Cancelled by driver',
    'CANCELLED_BY_ADMIN' => 'Cancelled by admin',
    'PROBLEM_REPORTED' => 'Problem reported',
    _ => status,
  };
}

Color _colorFor(String status) {
  if (_terminalStatuses.contains(status)) {
    return status == 'DELIVERED' ? Colors.green : Colors.red;
  }
  if (_activeStatuses.contains(status)) return primaryColor;
  return Colors.black38;
}

const _timelineStatuses = [
  'WAITING_DRIVER',
  'ASSIGNED',
  'ACCEPTED',
  'ARRIVED_AT_PICKUP',
  'PICKED_UP',
  'IN_TRANSIT',
  'ARRIVED_AT_DROPOFF',
  'DELIVERED',
];

// ── screen ─────────────────────────────────────────────────────────

class SpecificRequestScreen extends StatefulWidget {
  final String deliveryId;

  const SpecificRequestScreen({super.key, required this.deliveryId});

  @override
  State<SpecificRequestScreen> createState() => _SpecificRequestScreenState();
}

class _SpecificRequestScreenState extends State<SpecificRequestScreen> {
  Map<String, dynamic>? _delivery;
  bool _loading = true;
  bool _cancelling = false;
  Timer? _timer;

  @override
  void initState() {
    super.initState();
    WidgetsBinding.instance.addPostFrameCallback((_) async {
      await _fetchDelivery();
      _startPolling();
    });
  }

  @override
  void dispose() {
    _timer?.cancel();
    super.dispose();
  }

  void _startPolling() {
    _timer = Timer.periodic(const Duration(seconds: 5), (_) async {
      final status = _delivery?['deliveryStatus'] as String?;
      if (status != null && _terminalStatuses.contains(status)) {
        _timer?.cancel();
        return;
      }
      await _fetchDelivery();
    });
  }

  Future<void> _fetchDelivery() async {
    final authService = InheritedAuth.of(context).authService;
    final result = await DeliveryService.getDelivery(
      authService: authService,
      id: widget.deliveryId,
    );
    if (!mounted) return;
    if (result.isSuccess) {
      setState(() {
        _delivery = result.data;
        _loading = false;
      });
    } else {
      setState(() => _loading = false);
    }
  }

  Future<void> _cancel() async {
    final confirmed = await showDialog<bool>(
      context: context,
      builder: (context) => AlertDialog(
        title: const Text('Cancel delivery'),
        content: const Text('Are you sure you want to cancel this delivery?'),
        actions: [
          TextButton(
            onPressed: () => Navigator.pop(context, false),
            child: const Text('No'),
          ),
          TextButton(
            onPressed: () => Navigator.pop(context, true),
            child: Text('Yes', style: TextStyle(color: Colors.red)),
          ),
        ],
      ),
    );

    if (confirmed != true) return;

    setState(() => _cancelling = true);
    final authService = InheritedAuth.of(context).authService;
    final result = await DeliveryService.cancelDelivery(
      authService: authService,
      id: widget.deliveryId,
    );
    if (!mounted) return;
    setState(() => _cancelling = false);

    if (result.isSuccess) {
      await _fetchDelivery();
    } else {
      ScaffoldMessenger.of(context).showSnackBar(
        const SnackBar(content: Text('Could not cancel delivery')),
      );
    }
  }

  // ── build ──────────────────────────────────────────────────────

  @override
  Widget build(BuildContext context) {
    if (_loading) {
      return const Scaffold(body: Center(child: CircularProgressIndicator()));
    }

    if (_delivery == null) {
      return Scaffold(
        appBar: AppBar(),
        body: Center(child: Text('Could not load delivery', style: bodyText)),
      );
    }

    final status = _delivery!['deliveryStatus'] as String? ?? 'CREATED';
    final isTerminal = _terminalStatuses.contains(status);
    final canCancel = _cancellableStatuses.contains(status);
    final hasDriver = _delivery!['assignedDriver'] != null;

    final pickup = _delivery!['pickupAddress'] as Map<String, dynamic>?;
    final dropoff = _delivery!['deliveryAddress'] as Map<String, dynamic>?;
    final payment = _delivery!['payment'] as Map<String, dynamic>?;

    final pickupLatLng = pickup != null
        ? LatLng(pickup['latitude'] as double, pickup['longitude'] as double)
        : null;
    final dropoffLatLng = dropoff != null
        ? LatLng(dropoff['latitude'] as double, dropoff['longitude'] as double)
        : null;

    // placeholder driver location until endpoint exists
    final driverLatLng = hasDriver && pickupLatLng != null
        ? LatLng(pickupLatLng.latitude + 0.002, pickupLatLng.longitude + 0.002)
        : null;

    final mapCenter = pickupLatLng ?? const LatLng(33.5731, -7.5898);

    return Scaffold(
      backgroundColor: backgroundColor,
      body: Stack(
        children: [
          // ── map ────────────────────────────────────────────────
          SizedBox(
            height: MediaQuery.of(context).size.height * 0.45,
            child: FlutterMap(
              options: MapOptions(initialCenter: mapCenter, initialZoom: 13),
              children: [
                TileLayer(
                  urlTemplate: 'https://tile.openstreetmap.org/{z}/{x}/{y}.png',
                  userAgentPackageName: 'com.example.wasel',
                ),
                MarkerLayer(
                  markers: [
                    if (pickupLatLng != null)
                      Marker(
                        point: pickupLatLng,
                        child: const Icon(
                          Icons.circle,
                          color: primaryColor,
                          size: 16,
                        ),
                      ),
                    if (dropoffLatLng != null)
                      Marker(
                        point: dropoffLatLng,
                        child: const Icon(
                          Icons.location_on_rounded,
                          color: Colors.red,
                          size: 32,
                        ),
                      ),
                    if (driverLatLng != null)
                      Marker(
                        point: driverLatLng,
                        child: const Icon(
                          Icons.directions_car_rounded,
                          color: secondaryColor,
                          size: 28,
                        ),
                      ),
                  ],
                ),
              ],
            ),
          ),

          // ── back button ────────────────────────────────────────
          Positioned(
            top: MediaQuery.of(context).padding.top + 8,
            left: 16,
            child: CircleAvatar(
              backgroundColor: Colors.white,
              child: IconButton(
                icon: const Icon(Icons.arrow_back, color: secondaryColor),
                onPressed: () => Navigator.pop(context),
              ),
            ),
          ),

          // ── bottom sheet ───────────────────────────────────────
          DraggableScrollableSheet(
            initialChildSize: 0.6,
            minChildSize: 0.55,
            maxChildSize: 0.92,
            snap: true,
            snapSizes: const [0.55, 0.92],
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

                    // ── status header ───────────────────────────
                    Row(
                      mainAxisAlignment: MainAxisAlignment.spaceBetween,
                      children: [
                        Text(
                          _labelFor(status),
                          style: headingText.copyWith(color: _colorFor(status)),
                        ),
                        if (!isTerminal)
                          Text(
                            'Waiting',
                            style: captionText.copyWith(color: Colors.black38),
                          ),
                      ],
                    ),
                    const SizedBox(height: 20),

                    // ── driver card ─────────────────────────────
                    if (hasDriver) ...[
                      _DriverCard(driver: _delivery!['assignedDriver']),
                      const SizedBox(height: 20),
                    ] else if (!isTerminal) ...[
                      Container(
                        padding: const EdgeInsets.all(16),
                        decoration: BoxDecoration(
                          color: surfaceColor,
                          borderRadius: BorderRadius.circular(12),
                        ),
                        child: Row(
                          children: [
                            const Icon(
                              Icons.access_time_rounded,
                              color: Colors.black38,
                            ),
                            const SizedBox(width: 12),
                            Text(
                              'Looking for a nearby driver...',
                              style: bodyText.copyWith(color: Colors.black54),
                            ),
                          ],
                        ),
                      ),
                      const SizedBox(height: 20),
                    ],

                    // ── addresses ───────────────────────────────
                    _AddressRow(
                      icon: Icons.circle,
                      iconColor: primaryColor,
                      label: pickup?['street'] ?? '--',
                      sublabel: pickup?['city'] ?? '',
                    ),
                    const Padding(
                      padding: EdgeInsets.only(left: 10),
                      child: SizedBox(
                        height: 20,
                        child: VerticalDivider(width: 1, color: Colors.black26),
                      ),
                    ),
                    _AddressRow(
                      icon: Icons.location_on_rounded,
                      iconColor: Colors.red,
                      label: dropoff?['street'] ?? '--',
                      sublabel: dropoff?['city'] ?? '',
                    ),
                    const SizedBox(height: 20),

                    Divider(color: surfaceVariant),
                    const SizedBox(height: 16),

                    // ── timeline ────────────────────────────────
                    Text('Status', style: subHeadingText),
                    const SizedBox(height: 16),
                    _StatusTimeline(
                      currentStatus: status,
                      statusHistory:
                          _delivery!['statusHistory'] as List<dynamic>? ?? [],
                    ),

                    const SizedBox(height: 20),
                    Divider(color: surfaceVariant),
                    const SizedBox(height: 16),

                    // ── price ────────────────────────────────────
                    Row(
                      mainAxisAlignment: MainAxisAlignment.spaceBetween,
                      children: [
                        Text('Price', style: labelText),
                        Text(
                          '${(payment?['amount'] ?? 0.0).toStringAsFixed(2)} DH',
                          style: headingText,
                        ),
                      ],
                    ),

                    const SizedBox(height: 24),

                    // ── cancel button ────────────────────────────
                    if (canCancel)
                      OutlinedButton(
                        onPressed: _cancelling ? null : _cancel,
                        style: OutlinedButton.styleFrom(
                          padding: const EdgeInsets.symmetric(vertical: 14),
                          side: const BorderSide(color: Colors.red),
                          shape: RoundedRectangleBorder(
                            borderRadius: BorderRadius.circular(10),
                          ),
                        ),
                        child: _cancelling
                            ? const SizedBox(
                                height: 20,
                                width: 20,
                                child: CircularProgressIndicator(
                                  strokeWidth: 2,
                                  color: Colors.red,
                                ),
                              )
                            : Text(
                                'Cancel delivery',
                                style: bolderLabelText.copyWith(
                                  color: Colors.red,
                                ),
                              ),
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

// ── driver card ────────────────────────────────────────────────────

class _DriverCard extends StatelessWidget {
  final dynamic driver;
  const _DriverCard({required this.driver});

  @override
  Widget build(BuildContext context) {
    // TODO: update fields once assignedDriver shape is confirmed
    return Container(
      padding: const EdgeInsets.all(16),
      decoration: BoxDecoration(
        color: surfaceColor,
        borderRadius: BorderRadius.circular(12),
      ),
      child: Row(
        children: [
          CircleAvatar(
            radius: 24,
            backgroundColor: surfaceVariant,
            child: const Icon(Icons.person_rounded, color: secondaryColor),
          ),
          const SizedBox(width: 12),
          Expanded(
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                Text('Driver assigned', style: labelText),
                const SizedBox(height: 4),
                Text(
                  'Details coming soon',
                  style: captionText.copyWith(color: Colors.black38),
                ),
              ],
            ),
          ),
        ],
      ),
    );
  }
}

// ── address row ────────────────────────────────────────────────────

class _AddressRow extends StatelessWidget {
  final IconData icon;
  final Color iconColor;
  final String label;
  final String sublabel;

  const _AddressRow({
    required this.icon,
    required this.iconColor,
    required this.label,
    required this.sublabel,
  });

  @override
  Widget build(BuildContext context) {
    return Row(
      children: [
        Icon(icon, color: iconColor, size: 18),
        const SizedBox(width: 12),
        Expanded(
          child: Column(
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              Text(
                label,
                style: bodyText,
                maxLines: 1,
                overflow: TextOverflow.ellipsis,
              ),
              if (sublabel.isNotEmpty)
                Text(
                  sublabel,
                  style: captionText.copyWith(color: Colors.black38),
                ),
            ],
          ),
        ),
      ],
    );
  }
}

// ── status timeline ────────────────────────────────────────────────

class _StatusTimeline extends StatelessWidget {
  final String currentStatus;
  final List<dynamic> statusHistory;

  const _StatusTimeline({
    required this.currentStatus,
    required this.statusHistory,
  });

  @override
  Widget build(BuildContext context) {
    final currentIndex = _timelineStatuses.indexOf(currentStatus);

    return Column(
      children: List.generate(_timelineStatuses.length, (index) {
        final status = _timelineStatuses[index];
        final isDone = index <= currentIndex;
        final isCurrent = index == currentIndex;

        // find timestamp from history if available
        final historyEntry = statusHistory
            .cast<Map<String, dynamic>>()
            .where((h) => h['status'] == status)
            .firstOrNull;
        final timestamp = historyEntry?['changedAt'] as String?;

        return Row(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            // dot + line
            Column(
              children: [
                Container(
                  width: 20,
                  height: 20,
                  decoration: BoxDecoration(
                    shape: BoxShape.circle,
                    color: isDone ? primaryColor : surfaceVariant,
                    border: isCurrent
                        ? Border.all(color: primaryColor, width: 3)
                        : null,
                  ),
                  child: isDone && !isCurrent
                      ? const Icon(Icons.check, size: 12, color: Colors.white)
                      : null,
                ),
                if (index < _timelineStatuses.length - 1)
                  Container(
                    width: 2,
                    height: 32,
                    color: isDone ? primaryColor : surfaceVariant,
                  ),
              ],
            ),
            const SizedBox(width: 12),
            Padding(
              padding: const EdgeInsets.only(top: 2),
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  Text(
                    _labelFor(status),
                    style: labelText.copyWith(
                      color: isDone ? onSurface : Colors.black38,
                      fontWeight: isCurrent ? FontWeight.w600 : FontWeight.w400,
                    ),
                  ),
                  if (timestamp != null)
                    Text(
                      _formatTime(timestamp),
                      style: captionText.copyWith(color: Colors.black38),
                    ),
                  const SizedBox(height: 12),
                ],
              ),
            ),
          ],
        );
      }),
    );
  }

  String _formatTime(String iso) {
    try {
      final dt = DateTime.parse(iso).toLocal();
      final h = dt.hour.toString().padLeft(2, '0');
      final m = dt.minute.toString().padLeft(2, '0');
      return '$h:$m';
    } catch (_) {
      return '';
    }
  }
}
