import 'package:flutter/material.dart';
import 'package:wasel/themes/colors.dart';
import 'package:wasel/themes/text_styles.dart';

// ─────────────────────────────────────────────────────────────────
// MODÈLE LOCAL — représente une mission passée ou en cours
// retournée par GET /api/deliveries/my-missions
// Dans le vrai projet : lib/model/mission_model.dart
// ─────────────────────────────────────────────────────────────────
class DriverMission {
  final String id;
  final String pickupLabel;
  final String dropoffLabel;
  final String status;        // ex: DELIVERED, CANCELLED_BY_CLIENT...
  final double earnedAmount;
  final DateTime date;

  const DriverMission({
    required this.id,
    required this.pickupLabel,
    required this.dropoffLabel,
    required this.status,
    required this.earnedAmount,
    required this.date,
  });

  // Helper : est-ce que la mission est encore active (pas terminée) ?
  bool get isActive =>
      status != 'DELIVERED' &&
      !status.startsWith('CANCELLED');
}

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
  double get _monthlyEarnings =>
      _missions
          .where((m) => m.status == 'DELIVERED')
          .fold(0.0, (sum, m) => sum + m.earnedAmount);

  @override
  Widget build(BuildContext context) {
    // Sépare les missions actives des terminées/annulées
    final activeMissions = _missions.where((m) => m.isActive).toList();
    final historyMissions = _missions.where((m) => !m.isActive).toList();

    return Scaffold(
      backgroundColor: backgroundColor,
      body: SafeArea(
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [

            // ── Header avec titre et total gains du mois ──
            Padding(
              padding: const EdgeInsets.fromLTRB(24, 24, 24, 0),
              child: Row(
                mainAxisAlignment: MainAxisAlignment.spaceBetween,
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  Text('My Missions', style: headingText),
                  // Earnings badge — jaune pour mettre en avant les gains
                  Container(
                    padding: const EdgeInsets.symmetric(horizontal: 12, vertical: 6),
                    decoration: BoxDecoration(
                      color: primaryColor.withValues(alpha: 0.15),
                      borderRadius: BorderRadius.circular(20),
                    ),
                    child: Row(
                      mainAxisSize: MainAxisSize.min,
                      children: [
                        const Icon(Icons.account_balance_wallet_rounded, size: 16, color: primaryColor),
                        const SizedBox(width: 6),
                        Text(
                          '${_monthlyEarnings.toStringAsFixed(0)} DH this month',
                          style: captionText.copyWith(
                            color: secondaryColor,
                            fontWeight: FontWeight.w600,
                          ),
                        ),
                      ],
                    ),
                  ),
                ],
              ),
            ),

            const SizedBox(height: 20),

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
                              padding: const EdgeInsets.symmetric(horizontal: 6, vertical: 2),
                              decoration: BoxDecoration(
                                color: secondaryColor,
                                borderRadius: BorderRadius.circular(10),
                              ),
                              child: Text(
                                '${activeMissions.length}',
                                style: captionText.copyWith(color: Colors.white, fontSize: 10),
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

            // ── Contenu des onglets ──
            Expanded(
              child: TabBarView(
                controller: _tabController,
                children: [
                  // Onglet Active
                  _MissionList(
                    missions: activeMissions,
                    emptyMessage: 'No active missions',
                    emptySubMessage: 'Accept a delivery from the Home tab',
                    emptyIcon: Icons.moped_rounded,
                  ),
                  // Onglet History
                  _MissionList(
                    missions: historyMissions,
                    emptyMessage: 'No missions yet',
                    emptySubMessage: 'Your completed deliveries will appear here',
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

// ─────────────────────────────────────────────────────────────────
// LISTE DE MISSIONS — réutilisée pour les deux onglets
// Reçoit la liste filtrée, évite de dupliquer le code d'affichage
// ─────────────────────────────────────────────────────────────────
class _MissionList extends StatelessWidget {
  final List<DriverMission> missions;
  final String emptyMessage;
  final String emptySubMessage;
  final IconData emptyIcon;

  const _MissionList({
    required this.missions,
    required this.emptyMessage,
    required this.emptySubMessage,
    required this.emptyIcon,
  });

  @override
  Widget build(BuildContext context) {
    if (missions.isEmpty) {
      return Center(
        child: Column(
          mainAxisAlignment: MainAxisAlignment.center,
          children: [
            Icon(emptyIcon, size: 56, color: surfaceVariant),
            const SizedBox(height: 16),
            Text(emptyMessage, style: subHeadingText.copyWith(color: secondaryColor)),
            const SizedBox(height: 8),
            Text(
              emptySubMessage,
              style: captionText.copyWith(color: secondaryColor.withValues(alpha: 0.5)),
              textAlign: TextAlign.center,
            ),
          ],
        ),
      );
    }

    return ListView.separated(
      padding: const EdgeInsets.fromLTRB(24, 0, 24, 24),
      itemCount: missions.length,
      separatorBuilder: (_, __) => const SizedBox(height: 12),
      itemBuilder: (context, index) => _MissionCard(mission: missions[index]),
    );
  }
}

// ─────────────────────────────────────────────────────────────────
// CARTE D'UNE MISSION — affiche le résumé d'une mission
// Même style que les cards client dans les maquettes
// ─────────────────────────────────────────────────────────────────
class _MissionCard extends StatelessWidget {
  final DriverMission mission;
  const _MissionCard({required this.mission});

  @override
  Widget build(BuildContext context) {
    return Container(
      decoration: BoxDecoration(
        color: Colors.white,
        borderRadius: BorderRadius.circular(16),
        boxShadow: [
          BoxShadow(
            color: secondaryColor.withValues(alpha: 0.07),
            blurRadius: 10,
            offset: const Offset(0, 3),
          ),
        ],
      ),
      padding: const EdgeInsets.all(16),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [

          // ── Ligne 1 : statut + montant gagné ──
          Row(
            mainAxisAlignment: MainAxisAlignment.spaceBetween,
            children: [
              // Badge de statut coloré selon l'état de la mission
              _StatusBadge(status: mission.status),
              // Montant — 0 DH si annulée, montant en jaune si livrée ou active
              Text(
                mission.earnedAmount > 0
                    ? '+${mission.earnedAmount.toStringAsFixed(0)} DH'
                    : '—',
                style: bolderLabelText.copyWith(
                  color: mission.earnedAmount > 0 ? primaryColor : secondaryColor.withValues(alpha: 0.4),
                ),
              ),
            ],
          ),

          const SizedBox(height: 12),

          // ── Adresses ──
          Row(
            children: [
              const Icon(Icons.circle, size: 10, color: primaryColor),
              const SizedBox(width: 8),
              Expanded(
                child: Text(
                  mission.pickupLabel,
                  style: bodyText.copyWith(fontSize: 13),
                  maxLines: 1,
                  overflow: TextOverflow.ellipsis,
                ),
              ),
            ],
          ),
          const SizedBox(height: 4),
          Padding(
            padding: const EdgeInsets.only(left: 4),
            child: Container(width: 1.5, height: 12, color: surfaceVariant),
          ),
          const SizedBox(height: 4),
          Row(
            children: [
              const Icon(Icons.location_on_rounded, size: 14, color: Colors.red),
              const SizedBox(width: 6),
              Expanded(
                child: Text(
                  mission.dropoffLabel,
                  style: bodyText.copyWith(fontSize: 13),
                  maxLines: 1,
                  overflow: TextOverflow.ellipsis,
                ),
              ),
            ],
          ),

          const SizedBox(height: 10),

          // ── Date ──
          Text(
            _formatDate(mission.date),
            style: captionText.copyWith(color: secondaryColor.withValues(alpha: 0.5)),
          ),
        ],
      ),
    );
  }

  // Formate la date de façon lisible : "Today 10:42 AM" ou "23 May 2:30 PM"
  String _formatDate(DateTime date) {
    final now = DateTime.now();
    final isToday = date.year == now.year && date.month == now.month && date.day == now.day;
    final hour = date.hour.toString().padLeft(2, '0');
    final min = date.minute.toString().padLeft(2, '0');
    final timeStr = '$hour:$min';

    if (isToday) return 'Today $timeStr';

    const months = ['Jan','Feb','Mar','Apr','May','Jun','Jul','Aug','Sep','Oct','Nov','Dec'];
    return '${date.day} ${months[date.month - 1]} $timeStr';
  }
}

// ─────────────────────────────────────────────────────────────────
// BADGE DE STATUT — couleur différente selon le statut
// Centralise la logique de couleur pour ne pas la répéter
// ─────────────────────────────────────────────────────────────────
class _StatusBadge extends StatelessWidget {
  final String status;
  const _StatusBadge({required this.status});

  // Mapping statut API → label lisible + couleur
  ({String label, Color color, Color bg}) get _config => switch (status) {
    'ASSIGNED'          => (label: 'Assigned',     color: Colors.blue,   bg: Colors.blue.withValues(alpha: 0.1)),
    'ACCEPTED'          => (label: 'Accepted',     color: Colors.blue,   bg: Colors.blue.withValues(alpha: 0.1)),
    'ARRIVED_AT_PICKUP' => (label: 'At pickup',    color: Colors.orange, bg: Colors.orange.withValues(alpha: 0.1)),
    'PICKED_UP'         => (label: 'Collected',    color: Colors.orange, bg: Colors.orange.withValues(alpha: 0.1)),
    'IN_TRANSIT'        => (label: 'On the way',   color: primaryColor,  bg: primaryColor.withValues(alpha: 0.12)),
    'ARRIVED_AT_DROPOFF'=> (label: 'At dropoff',   color: Colors.purple, bg: Colors.purple.withValues(alpha: 0.1)),
    'DELIVERED'         => (label: 'Delivered',    color: Colors.green,  bg: Colors.green.withValues(alpha: 0.1)),
    _                   => (label: 'Cancelled',    color: Colors.red,    bg: Colors.red.withValues(alpha: 0.1)),
  };

  @override
  Widget build(BuildContext context) {
    final cfg = _config;
    return Container(
      padding: const EdgeInsets.symmetric(horizontal: 10, vertical: 4),
      decoration: BoxDecoration(
        color: cfg.bg,
        borderRadius: BorderRadius.circular(20),
      ),
      child: Text(
        cfg.label,
        style: captionText.copyWith(color: cfg.color, fontWeight: FontWeight.w600),
      ),
    );
  }
}