import 'package:flutter/material.dart';
import 'package:flutter_appauth/flutter_appauth.dart';
import 'package:wasel/main.dart';
import 'package:wasel/screens/main_screen.dart';
import 'package:wasel/themes/colors.dart';
import 'package:wasel/themes/text_styles.dart';
import 'package:wasel/widgets/wasel_logo_horizontal.dart';

class WelcomeScreen extends StatelessWidget {
  const WelcomeScreen({super.key});

  @override
  Widget build(BuildContext context) {
    final authService = InheritedAuth.of(context).authService;

    void navigateHome() {
      if (!context.mounted) return;
      Navigator.pushReplacement(
        context,
        MaterialPageRoute(builder: (context) => const MainScreen()),
      );
    }

    return Scaffold(
      backgroundColor: backgroundColor,
      body: SafeArea(
        child: SingleChildScrollView(
          padding: const EdgeInsets.symmetric(horizontal: 32.0),
          child: Column(
            crossAxisAlignment: CrossAxisAlignment.stretch,
            children: [
              const SizedBox(height: 24),
              const WaselLogoHorizontal(),
              const SizedBox(height: 12),
              Padding(
                padding: const EdgeInsets.only(right: 32),
                child: Image.asset(
                  'assets/welcome-image.png',
                  fit: BoxFit.contain,
                  height: 260,
                ),
              ),
              const SizedBox(height: 16),
              Text(
                'Move anything\nin minutes',
                textAlign: TextAlign.center,
                style: displayText.copyWith(fontSize: 28),
              ),
              const SizedBox(height: 12),
              Opacity(
                opacity: 0.5,
                child: Text(
                  'Reliable, fast, and secure delivery across the city. Just tap and track.',
                  textAlign: TextAlign.center,
                  style: labelText,
                ),
              ),
              const SizedBox(height: 40),
              ElevatedButton(
                onPressed: () async {
                  try {
                    await authService.register();
                    navigateHome();
                  } on FlutterAppAuthPlatformException catch (e) {
                    if (!context.mounted) return;
                    ScaffoldMessenger.of(context).showSnackBar(
                      SnackBar(
                        content: Text(e.message ?? 'Registration failed'),
                      ),
                    );
                  }
                },
                style: ButtonStyle(
                  shape: WidgetStatePropertyAll(
                    RoundedRectangleBorder(
                      borderRadius: BorderRadius.circular(10),
                    ),
                  ),
                  padding: const WidgetStatePropertyAll(
                    EdgeInsets.symmetric(vertical: 14),
                  ),
                  backgroundColor: WidgetStatePropertyAll(primaryColor),
                  foregroundColor: WidgetStatePropertyAll(onPrimary),
                  textStyle: WidgetStatePropertyAll(bolderLabelText),
                ),
                child: const Text('Join now'),
              ),
              const SizedBox(height: 12),
              ElevatedButton(
                onPressed: () async {
                  try {
                    await authService.login();
                    navigateHome();
                  } on FlutterAppAuthPlatformException catch (e) {
                    if (!context.mounted) return;
                    ScaffoldMessenger.of(context).showSnackBar(
                      SnackBar(content: Text(e.message ?? 'Login failed')),
                    );
                  }
                },
                style: ButtonStyle(
                  shape: WidgetStatePropertyAll(
                    RoundedRectangleBorder(
                      borderRadius: BorderRadius.circular(10),
                      side: BorderSide(color: primaryColorw600),
                    ),
                  ),
                  padding: const WidgetStatePropertyAll(
                    EdgeInsets.symmetric(vertical: 14),
                  ),
                  backgroundColor: WidgetStatePropertyAll(surfaceColor),
                  foregroundColor: WidgetStatePropertyAll(onSurface),
                  textStyle: WidgetStatePropertyAll(bolderLabelText),
                ),
                child: const Text('Sign in'),
              ),
              const SizedBox(height: 32),
            ],
          ),
        ),
      ),
    );
  }
}
