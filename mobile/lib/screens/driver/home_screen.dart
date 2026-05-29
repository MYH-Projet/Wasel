import 'package:flutter/material.dart';
import 'package:wasel/themes/colors.dart';
import 'package:wasel/themes/text_styles.dart';
import 'package:wasel/screens/driver/notifications_screen.dart';
import 'package:wasel/model/available_delivery_model.dart';
import 'package:wasel/screens/driver/widgets/mock_deliveries.dart';
import 'package:wasel/screens/driver/widgets/delivery_card.dart';
import 'package:wasel/screens/driver/widgets/active_mission_screen.dart';

// ─────────────────────────────────────────────────────────────────
// ÉCRAN PRINCIPAL DRIVER — liste des courses disponibles
// ─────────────────────────────────────────────────────────────────
class DriverHomeScreen extends StatefulWidget {
  const DriverHomeScreen({super.key});

  @override
  State<DriverHomeScreen> createState() => _DriverHomeScreenState();
}

class _DriverHomeScreenState extends State<DriverHomeScreen> {
  // _deliveries : liste affichée. Dans le vrai projet, viendra de l'API.
  List<AvailableDelivery> _deliveries = List.from(mockDeliveries);

  // _loading : contrôle l'affichage du spinner pendant le chargement/refresh
  bool _loading = false;

  // _acceptingId : garde l'id de la course en cours d'acceptation pour
  // afficher un spinner uniquement sur ce bouton, pas sur toute la page.
  String? _acceptingId;

  // ── appelé au pull-to-refresh ──
  // Dans le vrai projet : await sur GET /api/deliveries/available
  Future<void> _refresh() async {
    setState(() => _loading = true);
    await Future.delayed(const Duration(seconds: 1)); // simulation réseau
    setState(() {
      _deliveries = List.from(mockDeliveries);
      _loading = false;
    });
  }

  // ── appelé quand le livreur appuie sur "Accept" ──
  // Dans le vrai projet : POST /api/deliveries/{id}/accept
  // Si 409 → quelqu'un d'autre a pris la course → on retire la carte et on informe
  Future<void> _acceptDelivery(AvailableDelivery delivery) async {
    setState(() => _acceptingId = delivery.id);
    await Future.delayed(
      const Duration(milliseconds: 800),
    ); // simulation réseau

    if (!mounted) return;
    setState(() => _acceptingId = null);

    // On retire la course de la liste locale car elle n'est plus disponible
    setState(() => _deliveries.removeWhere((d) => d.id == delivery.id));

    // On ouvre l'écran de mission en cours
    Navigator.push(
      context,
      MaterialPageRoute(
        builder: (_) => ActiveMissionScreen(delivery: delivery),
      ),
    );
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      backgroundColor: backgroundColor,
      appBar: AppBar(
        backgroundColor: surfaceColor,
        elevation: 0,
        title: const Text('Deliveries'),

        actions: [
          IconButton(
            icon: const Icon(Icons.notifications_rounded),

            onPressed: () {
              Navigator.push(
                context,

                MaterialPageRoute(
                  builder: (_) => const DriverNotificationsScreen(),
                ),
              );
            },
          ),
          IconButton(
            onPressed: _loading ? null : _refresh,
            icon: _loading
                ? const SizedBox(
                    width: 20,
                    height: 20,
                    child: CircularProgressIndicator(
                      strokeWidth: 2,
                      color: secondaryColor,
                    ),
                  )
                : const Icon(Icons.refresh_rounded, color: secondaryColor),
          ),
        ],
      ),
      body: SafeArea(
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            // ── Corps : liste ou état vide ──
            Expanded(
              child: _deliveries.isEmpty
                  ? _buildEmptyState()
                  : RefreshIndicator(
                      // Pull-to-refresh pour recharger les courses dispo
                      onRefresh: _refresh,
                      color: primaryColor,
                      child: ListView.separated(
                        padding: const EdgeInsets.fromLTRB(16, 0, 16, 24),
                        itemCount: _deliveries.length,
                        separatorBuilder: (_, __) => const SizedBox(height: 12),
                        itemBuilder: (context, index) {
                          final delivery = _deliveries[index];
                          return DeliveryCard(
                            delivery: delivery,
                            isAccepting: _acceptingId == delivery.id,
                            onAccept: () => _acceptDelivery(delivery),
                          );
                        },
                      ),
                    ),
            ),
          ],
        ),
      ),
    );
  }

  // ── État vide : affiché quand aucune course n'est disponible ──
  Widget _buildEmptyState() {
    return Center(
      child: Column(
        mainAxisAlignment: MainAxisAlignment.center,
        children: [
          Icon(Icons.moped_rounded, size: 64, color: surfaceVariant),
          const SizedBox(height: 16),
          Text(
            'No deliveries nearby',
            style: subHeadingText.copyWith(color: secondaryColor),
          ),
          const SizedBox(height: 8),
          Text(
            'Pull down to refresh',
            style: captionText.copyWith(
              color: secondaryColor.withValues(alpha: 0.5),
            ),
          ),
        ],
      ),
    );
  }
}

// ─────────────────────────────────────────────────────────────────
// End of DriverHomeScreen
// ─────────────────────────────────────────────────────────────────
