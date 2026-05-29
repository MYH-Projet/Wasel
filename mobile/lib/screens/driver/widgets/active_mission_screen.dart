import 'package:flutter/material.dart';
import 'package:wasel/model/available_delivery_model.dart';
import 'package:wasel/themes/colors.dart';
import 'package:wasel/themes/text_styles.dart';
import 'package:wasel/screens/driver/widgets/address_row.dart';
import 'package:wasel/screens/driver/widgets/mission_step_model.dart';

// ─────────────────────────────────────────────────────────────────
// ÉCRAN MISSION EN COURS
// Ouvert après acceptation d'une course. Montre les étapes de la
// mission avec des boutons séquentiels (un seul visible à la fois)
// correspondant aux statuts : ACCEPTED → ARRIVED_AT_PICKUP →
// PICKED_UP → IN_TRANSIT → ARRIVED_AT_DROPOFF → DELIVERED
// ─────────────────────────────────────────────────────────────────
class ActiveMissionScreen extends StatefulWidget {
  final AvailableDelivery delivery;
  const ActiveMissionScreen({super.key, required this.delivery});

  @override
  State<ActiveMissionScreen> createState() => _ActiveMissionScreenState();
}

class _ActiveMissionScreenState extends State<ActiveMissionScreen> {
  // _stepIndex : indice de l'étape courante dans missionSteps
  // 0 = ACCEPTED, 4 = ARRIVED_AT_DROPOFF, 5 = terminé
  int _stepIndex = 0;
  bool _updating = false; // spinner sur le bouton pendant l'appel API

  // ── appelé à chaque bouton d'étape ──
  // Dans le vrai projet : PATCH /api/deliveries/{id}/status
  Future<void> _nextStep() async {
    setState(() => _updating = true);
    await Future.delayed(const Duration(milliseconds: 600)); // simulation
    if (!mounted) return;

    if (_stepIndex >= missionSteps.length - 1) {
      // Dernière étape = DELIVERED → on ferme l'écran
      setState(() => _updating = false);
      Navigator.pop(context);
      ScaffoldMessenger.of(context).showSnackBar(
        const SnackBar(
          content: Text('Delivery completed!'),
          backgroundColor: secondaryColor,
        ),
      );
    } else {
      setState(() {
        _stepIndex++;
        _updating = false;
      });
    }
  }

  @override
  Widget build(BuildContext context) {
    final currentStep = missionSteps[_stepIndex];
    final isLast = _stepIndex == missionSteps.length - 1;

    return Scaffold(
      backgroundColor: backgroundColor,
      appBar: AppBar(
        backgroundColor: backgroundColor,
        elevation: 0,
        leading: IconButton(
          // WillPopScope serait mieux ici pour confirmer avant de quitter
          // mais on garde simple pour ce sprint
          icon: const Icon(Icons.arrow_back_rounded, color: secondaryColor),
          onPressed: () => Navigator.pop(context),
        ),
        title: Text('Active Mission', style: subHeadingText),
        centerTitle: true,
      ),
      body: SafeArea(
        child: Padding(
          padding: const EdgeInsets.symmetric(horizontal: 24),
          child: Column(
            crossAxisAlignment: CrossAxisAlignment.stretch,
            children: [
              const SizedBox(height: 16),

              // ── Carte infos livraison ──
              Container(
                decoration: BoxDecoration(
                  color: surfaceColor,
                  borderRadius: BorderRadius.circular(16),
                ),
                padding: const EdgeInsets.all(16),
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    AddressRow(
                      icon: Icons.circle,
                      iconColor: primaryColor,
                      label: widget.delivery.pickupLabel,
                    ),
                    Padding(
                      padding: const EdgeInsets.only(left: 8),
                      child: Container(
                        width: 1.5,
                        height: 16,
                        color: surfaceVariant,
                      ),
                    ),
                    AddressRow(
                      icon: Icons.location_on_rounded,
                      iconColor: primaryColor,
                      label: widget.delivery.dropoffLabel,
                    ),
                    const SizedBox(height: 12),
                    Row(
                      children: [
                        // Earnings
                        Text(
                          '${widget.delivery.price.toStringAsFixed(0)} DH',
                          style: bolderLabelText.copyWith(
                            color: secondaryColor,
                          ),
                        ),
                        const SizedBox(width: 12),
                        Text(
                          '·',
                          style: bodyText.copyWith(color: surfaceVariant),
                        ),
                        const SizedBox(width: 12),
                        Text(
                          '${widget.delivery.distanceKm} km',
                          style: captionText.copyWith(color: secondaryColor),
                        ),
                        if (widget.delivery.isFragile) ...[
                          const SizedBox(width: 8),
                          const Icon(
                            Icons.warning_amber_rounded,
                            size: 14,
                            color: secondaryColor,
                          ),
                          const SizedBox(width: 2),
                          Text(
                            'Fragile',
                            style: captionText.copyWith(color: secondaryColor),
                          ),
                        ],
                      ],
                    ),
                  ],
                ),
              ),

              const SizedBox(height: 32),

              // ── Timeline des étapes ──
              // Affiche toutes les étapes passées (cochées) et la courante
              Expanded(
                child: ListView.builder(
                  itemCount: missionSteps.length,
                  itemBuilder: (context, index) {
                    final step = missionSteps[index];
                    final isDone = index < _stepIndex;
                    final isCurrent = index == _stepIndex;

                    return Padding(
                      padding: const EdgeInsets.only(bottom: 8),
                      child: Row(
                        children: [
                          // Icône : cochée si done, colorée si current, grise si future
                          Container(
                            width: 36,
                            height: 36,
                            decoration: BoxDecoration(
                              color: isDone
                                  ? primaryColor
                                  : isCurrent
                                  ? primaryColor.withValues(alpha: 0.15)
                                  : surfaceColor,
                              shape: BoxShape.circle,
                            ),
                            child: Icon(
                              isDone ? Icons.check_rounded : step.icon,
                              size: 18,
                              color: isDone
                                  ? secondaryColor
                                  : isCurrent
                                  ? primaryColor
                                  : surfaceVariant,
                            ),
                          ),
                          const SizedBox(width: 12),
                          Text(
                            step.statusLabel,
                            style: bodyText.copyWith(
                              color: isDone || isCurrent
                                  ? onBackground
                                  : secondaryColor.withValues(alpha: 0.4),
                              fontWeight: isCurrent
                                  ? FontWeight.w600
                                  : FontWeight.w400,
                            ),
                          ),
                        ],
                      ),
                    );
                  },
                ),
              ),

              // ── Bouton d'étape ──
              // Label change à chaque étape. Dernier = "Delivered ✓" en vert
              ElevatedButton(
                onPressed: _updating ? null : _nextStep,
                style: ElevatedButton.styleFrom(
                  backgroundColor: isLast ? secondaryColor : primaryColor,
                  foregroundColor: isLast ? Colors.white : secondaryColor,
                  padding: const EdgeInsets.symmetric(vertical: 16),
                  shape: RoundedRectangleBorder(
                    borderRadius: BorderRadius.circular(12),
                  ),
                  elevation: 0,
                ),
                child: _updating
                    ? SizedBox(
                        width: 22,
                        height: 22,
                        child: CircularProgressIndicator(
                          strokeWidth: 2.5,
                          color: isLast ? Colors.white : secondaryColor,
                        ),
                      )
                    : Text(
                        currentStep.buttonLabel,
                        style: bolderLabelText.copyWith(
                          color: isLast ? Colors.white : secondaryColor,
                        ),
                      ),
              ),

              const SizedBox(height: 24),
            ],
          ),
        ),
      ),
    );
  }
}
