import 'package:flutter/material.dart';
import 'package:mobile/screens/register_screen.dart';
import 'package:mobile/themes/colors.dart';
import 'package:mobile/themes/text_styles.dart';
import 'package:mobile/widgets/wasel_logo_horizontal.dart';

class WelcomeScreen extends StatelessWidget {
  const WelcomeScreen({required this.userName, super.key});

  final String userName;

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      body: Padding(
        padding: EdgeInsets.symmetric(horizontal: 32.0, vertical: 16),
        child: Align(
          alignment: Alignment.center,
          child: Column(
            spacing: 16,
            crossAxisAlignment: CrossAxisAlignment.stretch,
            children: [
              SizedBox(height: 16),
              WaselLogoHorizontal(),
              Image.asset('./assets/welcome-image.png'),
              Text(
                'Move anything in minutes',
                textAlign: TextAlign.center,
                style: displayText.copyWith(fontSize: 28),
              ),
              Opacity(
                opacity: 0.5,
                child: Text(
                  'Reliable, fast, and secure delivery across the city. Just tap and track.',
                  textAlign: TextAlign.center,
                  style: labelText,
                ),
              ),
              ElevatedButton(
                onPressed: () {
                  Navigator.push(
                    context,
                    MaterialPageRoute<void>(
                      builder: (context) => const RegisterScreen(),
                    ),
                  );
                },
                style: ButtonStyle(
                  shape: WidgetStatePropertyAll<RoundedRectangleBorder>(
                    RoundedRectangleBorder(
                      borderRadius: BorderRadius.all(Radius.circular(10)),
                    ),
                  ),
                  padding: WidgetStatePropertyAll(
                    EdgeInsets.symmetric(vertical: 12),
                  ),
                  backgroundColor: WidgetStatePropertyAll(primaryColor),
                  foregroundColor: WidgetStatePropertyAll(onPrimary),
                  textStyle: WidgetStatePropertyAll(bolderLabelText),
                ),
                child: Text('Join now'),
              ),
              ElevatedButton(
                onPressed: null,
                style: ButtonStyle(
                  shape: WidgetStatePropertyAll<RoundedRectangleBorder>(
                    RoundedRectangleBorder(
                      borderRadius: BorderRadius.all(Radius.circular(10)),
                      side: BorderSide(color: primaryColorw600),
                    ),
                  ),
                  padding: WidgetStatePropertyAll(
                    EdgeInsets.symmetric(vertical: 12),
                  ),
                  backgroundColor: WidgetStatePropertyAll(surfaceColor),
                  foregroundColor: WidgetStatePropertyAll(onSurface),
                  textStyle: WidgetStatePropertyAll(bolderLabelText),
                ),
                child: Text('Sign in'),
              ),
            ],
          ),
        ),
      ),
      backgroundColor: backgroundColor,
    );
  }
}
