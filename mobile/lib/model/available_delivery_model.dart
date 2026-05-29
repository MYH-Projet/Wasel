// ─────────────────────────────────────────────────────────────────
// MODÈLE LOCAL — représente une course disponible retournée par
// GET /api/deliveries/available. Dans le vrai projet tu le peupleras
// depuis l'API.
// ─────────────────────────────────────────────────────────────────
class AvailableDelivery {
  final String id;
  final String pickupLabel;
  final String dropoffLabel;
  final double distanceKm;
  final double price;
  final double weightKg;
  final bool isFragile;

  const AvailableDelivery({
    required this.id,
    required this.pickupLabel,
    required this.dropoffLabel,
    required this.distanceKm,
    required this.price,
    required this.weightKg,
    required this.isFragile,
  });
}
