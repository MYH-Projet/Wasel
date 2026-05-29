import 'package:flutter/material.dart';
import 'package:wasel/model/available_delivery_model.dart';
import 'package:wasel/themes/colors.dart';
import 'package:wasel/themes/text_styles.dart';
import 'package:wasel/screens/driver/widgets/address_row.dart';

// ─────────────────────────────────────────────────────────────────
// CARTE D'UNE COURSE DISPONIBLE
// Widget séparé pour garder le code lisible. Reçoit la course
// et un callback onAccept pour remonter l'action au parent.
// ─────────────────────────────────────────────────────────────────
class DeliveryCard extends StatelessWidget {
  final AvailableDelivery delivery;
  final bool isAccepting; // true → affiche spinner sur ce bouton
  final VoidCallback onAccept;

  const DeliveryCard({
    required this.delivery,
    required this.isAccepting,
    required this.onAccept,
  });

  @override
  Widget build(BuildContext context) {
    return Container(
      decoration: BoxDecoration(
        color: Colors.white,
        borderRadius: BorderRadius.circular(16),
        // Ombre légère — même style que les cards dans les maquettes client
        boxShadow: [
          BoxShadow(
            color: secondaryColor.withValues(alpha: 0.08),
            blurRadius: 12,
            offset: const Offset(0, 4),
          ),
        ],
      ),
      padding: const EdgeInsets.all(16),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          // ── Ligne prix + distance ──
          Row(
            mainAxisAlignment: MainAxisAlignment.spaceBetween,
            children: [
              // Prix en gros, jaune — met en avant le gain potentiel
              Text(
                '${delivery.price.toStringAsFixed(0)} DH',
                style: headingText.copyWith(color: secondaryColor),
              ),
              // Distance + poids dans un chip léger
              Container(
                padding: const EdgeInsets.symmetric(
                  horizontal: 10,
                  vertical: 4,
                ),
                decoration: BoxDecoration(
                  color: surfaceColor,
                  borderRadius: BorderRadius.circular(20),
                ),
                child: Text(
                  '${delivery.distanceKm} km · ${delivery.weightKg} kg',
                  style: captionText.copyWith(color: secondaryColor),
                ),
              ),
            ],
          ),
          const SizedBox(height: 14),

          // ── Adresses pickup et dropoff ──
          // Même pattern visuel que le tracking screen des maquettes client :
          // point jaune = départ, point rouge = arrivée, ligne verticale entre les deux
          AddressRow(
            icon: Icons.circle,
            iconColor: primaryColor,
            label: delivery.pickupLabel,
          ),
          // Ligne verticale connectant les deux points
          Padding(
            padding: const EdgeInsets.only(left: 8),
            child: Container(width: 1.5, height: 16, color: surfaceVariant),
          ),
          AddressRow(
            icon: Icons.location_on_rounded,
            iconColor: primaryColor,
            label: delivery.dropoffLabel,
          ),

          const SizedBox(height: 14),

          // ── Fragile badge + bouton Accept ──
          Row(
            children: [
              // Badge "Fragile" visible seulement si isFragile = true
              if (delivery.isFragile) ...[
                Container(
                  padding: const EdgeInsets.symmetric(
                    horizontal: 10,
                    vertical: 4,
                  ),
                  decoration: BoxDecoration(
                    color: secondaryColor.withValues(alpha: 0.12),
                    borderRadius: BorderRadius.circular(20),
                  ),
                  child: Row(
                    mainAxisSize: MainAxisSize.min,
                    children: [
                      const Icon(
                        Icons.warning_amber_rounded,
                        size: 14,
                        color: secondaryColor,
                      ),
                      const SizedBox(width: 4),
                      Text(
                        'Fragile',
                        style: captionText.copyWith(color: secondaryColor),
                      ),
                    ],
                  ),
                ),
                const SizedBox(width: 8),
              ],

              const Spacer(),

              // Bouton Accept — devient spinner pendant l'appel API
              SizedBox(
                height: 40,
                child: ElevatedButton(
                  onPressed: isAccepting ? null : onAccept,
                  style: ElevatedButton.styleFrom(
                    backgroundColor: primaryColor,
                    foregroundColor: secondaryColor,
                    padding: const EdgeInsets.symmetric(horizontal: 24),
                    shape: RoundedRectangleBorder(
                      borderRadius: BorderRadius.circular(10),
                    ),
                    elevation: 0,
                  ),
                  child: isAccepting
                      ? const SizedBox(
                          width: 18,
                          height: 18,
                          child: CircularProgressIndicator(
                            strokeWidth: 2,
                            color: secondaryColor,
                          ),
                        )
                      : Text(
                          'Accept',
                          style: bolderLabelText.copyWith(
                            color: secondaryColor,
                          ),
                        ),
                ),
              ),
            ],
          ),
        ],
      ),
    );
  }
}
