import 'package:wasel/model/available_delivery_model.dart';

// ─────────────────────────────────────────────────────────────────
// DONNÉES FICTIVES — à remplacer par l'appel API réel
// ─────────────────────────────────────────────────────────────────
final mockDeliveries = [
  const AvailableDelivery(
    id: '1',
    pickupLabel: 'Café Atlas, Rue Ibn Batouta',
    dropoffLabel: '12 Av. Mohammed V, Tanger',
    distanceKm: 3.2,
    price: 18,
    weightKg: 1.5,
    isFragile: false,
  ),
  const AvailableDelivery(
    id: '2',
    pickupLabel: 'Marché Central, Tanger',
    dropoffLabel: 'Résidence Al Bahr, Malabata',
    distanceKm: 5.8,
    price: 25,
    weightKg: 4.0,
    isFragile: true,
  ),
  const AvailableDelivery(
    id: '3',
    pickupLabel: 'Pharmacie Ennour, Hay Saddam',
    dropoffLabel: '7 Rue de Fès, Centre-ville',
    distanceKm: 2.1,
    price: 12,
    weightKg: 0.5,
    isFragile: false,
  ),
  const AvailableDelivery(
    id: '4',
    pickupLabel: 'Pharmacie Ennour, Hay Saddam',
    dropoffLabel: '7 Rue de Fès, Centre-ville',
    distanceKm: 2.1,
    price: 12,
    weightKg: 0.5,
    isFragile: false,
  ),
  const AvailableDelivery(
    id: '5',
    pickupLabel: 'Pharmacie Ennour, Hay Saddam',
    dropoffLabel: '7 Rue de Fès, Centre-ville',
    distanceKm: 2.1,
    price: 12,
    weightKg: 0.5,
    isFragile: false,
  ),
];
