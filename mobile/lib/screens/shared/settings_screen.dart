import 'package:flutter/material.dart';
import 'package:wasel/main.dart';
import 'package:wasel/screens/welcome_screen.dart';
import 'package:wasel/themes/colors.dart';
import 'package:wasel/themes/text_styles.dart';

class SettingsScreen extends StatelessWidget {
  final bool isDriver;
  final VoidCallback onModeSwitch;

  const SettingsScreen({
    super.key,
    required this.isDriver,
    required this.onModeSwitch,
  });

  @override
  Widget build(BuildContext context) {
    final authService = InheritedAuth.of(context).authService;

    return Scaffold(
      backgroundColor: backgroundColor,
      body: SafeArea(
        child: Padding(
          padding: const EdgeInsets.symmetric(horizontal: 24),
          child: Column(
            crossAxisAlignment: CrossAxisAlignment.stretch,
            children: [
              const SizedBox(height: 32),
              Text('Settings', style: headingText),
              const SizedBox(height: 32),
              OutlinedButton.icon(
                onPressed: onModeSwitch,
                icon: Icon(
                  isDriver ? Icons.person_rounded : Icons.drive_eta_rounded,
                  color: secondaryColor,
                ),
                label: Text(
                  isDriver ? 'Switch to Client Mode' : 'Switch to Driver Mode',
                  style: bolderLabelText.copyWith(color: secondaryColor),
                ),
                style: OutlinedButton.styleFrom(
                  padding: const EdgeInsets.symmetric(vertical: 14),
                  side: BorderSide(color: surfaceVariant),
                  shape: RoundedRectangleBorder(
                    borderRadius: BorderRadius.circular(10),
                  ),
                ),
              ),
              const Spacer(),
              OutlinedButton.icon(
                onPressed: () async {
                  await authService.clearTokens();
                  if (!context.mounted) return;
                  Navigator.pushReplacement(
                    context,
                    MaterialPageRoute(
                      builder: (context) => const WelcomeScreen(),
                    ),
                  );
                },
                icon: const Icon(Icons.logout_rounded, color: Colors.red),
                label: Text(
                  'Logout',
                  style: bolderLabelText.copyWith(color: Colors.red),
                ),
                style: OutlinedButton.styleFrom(
                  padding: const EdgeInsets.symmetric(vertical: 14),
                  side: const BorderSide(color: Colors.red),
                  shape: RoundedRectangleBorder(
                    borderRadius: BorderRadius.circular(10),
                  ),
                ),
              ),
              const SizedBox(height: 32),
            ],
          ),
        ),
      ),
    );
  }
}
