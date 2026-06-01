import 'package:flutter/material.dart';
import 'package:wasel/themes/colors.dart';
import 'package:wasel/themes/text_styles.dart';
import 'package:wasel/screens/driver/widgets/mission_card.dart';
import 'package:wasel/screens/driver/widgets/mission_list.dart';
import 'package:wasel/screens/driver/widgets/driver_map.dart';
import 'package:latlong2/latlong.dart';
import 'package:flutter_map/flutter_map.dart';
import 'package:wasel/screens/driver/wallet_screen.dart';

// ─────────────────────────────────────────────────────────────────
// DONNÉES FICTIVES — à remplacer par l'appel API réel
// ─────────────────────────────────────────────────────────────────
final _mockMissions = [
  DriverMission(
    id: '101',
    pickupLabel: 'Café Atlas, Rue Ibn Batouta',
    dropoffLabel: '12 Av. Mohammed V, Tanger',
    status: 'IN_TRANSIT',
    earnedAmount: 18,
    date: DateTime.now().subtract(const Duration(minutes: 20)),
  ),
  DriverMission(
    id: '102',
    pickupLabel: 'Marché Central, Tanger',
    dropoffLabel: 'Résidence Al Bahr, Malabata',
    status: 'DELIVERED',
    earnedAmount: 25,
    date: DateTime.now().subtract(const Duration(hours: 3)),
  ),
  DriverMission(
    id: '103',
    pickupLabel: 'Pharmacie Ennour',
    dropoffLabel: '7 Rue de Fès',
    status: 'DELIVERED',
    earnedAmount: 12,
    date: DateTime.now().subtract(const Duration(days: 1)),
  ),
  DriverMission(
    id: '104',
    pickupLabel: 'Supermarché Label Vie',
    dropoffLabel: 'Cité Riad, Tanger',
    status: 'CANCELLED_BY_CLIENT',
    earnedAmount: 0,
    date: DateTime.now().subtract(const Duration(days: 2)),
  ),
];

// ─────────────────────────────────────────────────────────────────
// ÉCRAN REQUESTS DU DRIVER
// Deux onglets : Active (missions en cours) et History (terminées)
// Même pattern que les maquettes du client (onglets Active / Drafts)
// ─────────────────────────────────────────────────────────────────
class DriverRequestsScreen extends StatefulWidget {
  const DriverRequestsScreen({super.key});

  @override
  State<DriverRequestsScreen> createState() => _DriverRequestsScreenState();
}

class _DriverRequestsScreenState extends State<DriverRequestsScreen>
    with SingleTickerProviderStateMixin {
  // TabController pour gérer les onglets Active / History
  // SingleTickerProviderStateMixin est requis par TabController
  late final TabController _tabController;

  final List<DriverMission> _missions = List.from(_mockMissions);

  @override
  void initState() {
    super.initState();
    // 2 onglets : Active et History
    _tabController = TabController(length: 2, vsync: this);
  }

  @override
  void dispose() {
    // Toujours dispose le TabController pour éviter les memory leaks
    _tabController.dispose();
    super.dispose();
  }

  // Calcul du total des gains du mois — affiché en haut de l'écran
  // Dans le vrai projet, ce chiffre viendra directement de l'API
  double get _monthlyEarnings => _missions
      .where((m) => m.status == 'DELIVERED')
      .fold(0.0, (sum, m) => sum + m.earnedAmount);

  @override
  Widget build(BuildContext context) {
    // Sépare les missions actives des terminées/annulées
    final activeMissions = _missions.where((m) => m.isActive).toList();
    final historyMissions = _missions.where((m) => !m.isActive).toList();

    return Scaffold(
      backgroundColor: backgroundColor,
      appBar: AppBar(
        title: const Text('Missions'),
        backgroundColor: surfaceColor,
        elevation: 0,
        actions: [
          IconButton(
            icon: const Icon(Icons.account_balance_wallet_rounded),
            onPressed: () {
              Navigator.push(
                context,
                MaterialPageRoute(builder: (_) => const DriverWalletScreen()),
              );
            },
          ),
        ],
      ),
      body: SafeArea(
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            // ── Tab Bar — Active / History ──
            // Copiée exactement du design maquettes (Active / Drafts)
            // mais avec History à la place de Drafts pour le driver
            Padding(
              padding: const EdgeInsets.symmetric(horizontal: 24),
              child: Container(
                decoration: BoxDecoration(
                  color: surfaceColor,
                  borderRadius: BorderRadius.circular(12),
                ),
                child: TabBar(
                  controller: _tabController,
                  indicator: BoxDecoration(
                    color: primaryColor,
                    borderRadius: BorderRadius.circular(10),
                  ),
                  indicatorSize: TabBarIndicatorSize.tab,
                  dividerColor: Colors.transparent,
                  labelColor: secondaryColor,
                  unselectedLabelColor: secondaryColor.withValues(alpha: 0.5),
                  labelStyle: bolderLabelText.copyWith(fontSize: 14),
                  unselectedLabelStyle: bodyText.copyWith(fontSize: 14),
                  tabs: [
                    Tab(
                      child: Row(
                        mainAxisAlignment: MainAxisAlignment.center,
                        children: [
                          const Text('Active'),
                          // Badge avec le nombre de missions actives
                          if (activeMissions.isNotEmpty) ...[
                            const SizedBox(width: 6),
                            Container(
                              padding: const EdgeInsets.symmetric(
                                horizontal: 6,
                                vertical: 2,
                              ),
                              decoration: BoxDecoration(
                                color: secondaryColor,
                                borderRadius: BorderRadius.circular(10),
                              ),
                              child: Text(
                                '${activeMissions.length}',
                                style: captionText.copyWith(
                                  color: Colors.white,
                                  fontSize: 10,
                                ),
                              ),
                            ),
                          ],
                        ],
                      ),
                    ),
                    const Tab(text: 'History'),
                  ],
                ),
              ),
            ),
            const SizedBox(height: 16),

            // Small map preview showing active missions as markers
            Padding(
              padding: const EdgeInsets.fromLTRB(24, 16, 24, 0),
              child: DriverMap(
                center: LatLng(35.7595, -5.83395),
                height: 140,
                markers: List.generate(activeMissions.length, (i) {
                  final offset = 0.002 * (i + 1);
                  return Marker(
                    point: LatLng(35.7595 + offset, -5.83395 - offset),
                    width: 28,
                    height: 28,
                    child: const Icon(
                      Icons.location_on_rounded,
                      color: Colors.red,
                      size: 18,
                    ),
                  );
                }),
              ),
            ),

            const SizedBox(height: 16),

            // ── Contenu des onglets ──
            Expanded(
              child: TabBarView(
                controller: _tabController,
                children: [
                  // Onglet Active
                  MissionList(
                    missions: activeMissions,
                    emptyMessage: 'No active missions',
                    emptySubMessage: 'Accept a delivery from the Home tab',
                    emptyIcon: Icons.moped_rounded,
                  ),
                  // Onglet History
                  MissionList(
                    missions: historyMissions,
                    emptyMessage: 'No missions yet',
                    emptySubMessage:
                        'Your completed deliveries will appear here',
                    emptyIcon: Icons.history_rounded,
                  ),
                ],
              ),
            ),
          ],
        ),
      ),
    );
  }
}
