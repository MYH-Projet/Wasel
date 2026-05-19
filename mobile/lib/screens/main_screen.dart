import 'package:flutter/material.dart';
import 'package:wasel/api/auth_service.dart';
import 'package:wasel/main.dart';
import 'package:wasel/screens/client/home_screen.dart';
import 'package:wasel/screens/client/requests_screen.dart';
import 'package:wasel/screens/driver/home_screen.dart';
import 'package:wasel/screens/driver/requests_screen.dart';
import 'package:wasel/screens/shared/settings_screen.dart';
import 'package:wasel/widgets/wasel_bottom_bar.dart';

class MainScreen extends StatefulWidget {
  const MainScreen({super.key});

  @override
  State<MainScreen> createState() => _MainScreenState();
}

class _MainScreenState extends State<MainScreen> {
  int _currentIndex = 0;
  bool _isDriver = false;
  bool _loading = true;

  @override
  void initState() {
    super.initState();
    WidgetsBinding.instance.addPostFrameCallback((_) => _loadMode());
  }

  Future<void> _loadMode() async {
    final authService = InheritedAuth.of(context).authService;
    final mode = await authService.getMode();
    if (!context.mounted) return;
    setState(() {
      _isDriver = mode == UserMode.driver;
      _loading = false;
    });
  }

  Future<void> _switchMode() async {
    final authService = InheritedAuth.of(context).authService;
    final newMode = _isDriver ? UserMode.client : UserMode.driver;
    await authService.setMode(newMode);
    if (!context.mounted) return;
    setState(() {
      _isDriver = newMode == UserMode.driver;
      _currentIndex = 0;
    });
  }

  @override
  Widget build(BuildContext context) {
    if (_loading) {
      return const Scaffold(body: Center(child: CircularProgressIndicator()));
    }

    final screens = [
      _isDriver ? const DriverHomeScreen() : const ClientHomeScreen(),
      _isDriver ? const DriverRequestsScreen() : const ClientRequestsScreen(),
      SettingsScreen(isDriver: _isDriver, onModeSwitch: _switchMode),
    ];

    return Scaffold(
      body: IndexedStack(index: _currentIndex, children: screens),
      bottomNavigationBar: WaselBottomBar(
        currentIndex: _currentIndex,
        onTabSelected: (index) => setState(() => _currentIndex = index),
      ),
    );
  }
}
