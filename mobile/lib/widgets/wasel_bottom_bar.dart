import 'package:flutter/material.dart';
import 'package:wasel/themes/colors.dart';
import 'package:wasel/themes/text_styles.dart';

class WaselBottomBar extends StatefulWidget {
  const WaselBottomBar({super.key});

  @override
  State<WaselBottomBar> createState() => _WaselBottomBarState();
}

class _WaselBottomBarState extends State<WaselBottomBar> {
  int _currentIndex = 0;

  @override
  Widget build(BuildContext context) {
    return BottomNavigationBar(
      currentIndex: _currentIndex,
      onTap: (index) => setState(() => _currentIndex = index),
      backgroundColor: surfaceColor,
      selectedItemColor: primaryColor,
      unselectedItemColor: secondaryColor.withValues(alpha: 0.5),
      selectedLabelStyle: captionText.copyWith(color: primaryColor),
      unselectedLabelStyle: captionText.copyWith(
        color: secondaryColor.withValues(alpha: 0.5),
      ),
      type: BottomNavigationBarType.fixed,
      elevation: 0,
      items: const [
        BottomNavigationBarItem(icon: Icon(Icons.home_rounded), label: 'Home'),
        BottomNavigationBarItem(
          icon: Icon(Icons.list_alt_rounded),
          label: 'Requests',
        ),
        BottomNavigationBarItem(
          icon: Icon(Icons.settings_rounded),
          label: 'Settings',
        ),
      ],
    );
  }
}
