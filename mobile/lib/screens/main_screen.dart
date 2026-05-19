import 'package:flutter/material.dart';
import 'package:wasel/screens/client/home_screen.dart';
import 'package:wasel/screens/client/requests_screen.dart';
import 'package:wasel/screens/client/settings_screen.dart';
import 'package:wasel/widgets/wasel_bottom_bar.dart';

class MainScreen extends StatefulWidget {
  const MainScreen({super.key});

  @override
  State<MainScreen> createState() => _MainScreenState();
}

class _MainScreenState extends State<MainScreen> {
  int _currentIndex = 0;

  final List<Widget> _screens = const [
    ClientHomeScreen(),
    ClientRequestsScreen(),
    ClientSettingsScreen(),
  ];

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      body: IndexedStack(index: _currentIndex, children: _screens),
      bottomNavigationBar: WaselBottomBar(
        currentIndex: _currentIndex,
        onTabSelected: (index) => setState(() => _currentIndex = index),
      ),
    );
  }
}
