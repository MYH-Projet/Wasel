import 'package:flutter/material.dart';
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
            crossAxisAlignment: CrossAxisAlignment.center,
            children: [
              SizedBox(height: 16),
              WaselLogoHorizontal(),
              Image.asset('./assets/welcome-image.png'),
              Text(
                'Move anything in minutes',
                textAlign: TextAlign.center,
                style: headingText,
              ),
              Text(
                'Reliable, fast, and secure delivery across the city. Just tap and track.',
                textAlign: TextAlign.center,
                style: subHeadingText,
              ),
              ElevatedButton(
                onPressed: null,
                style: ButtonStyle(
                  backgroundColor: WidgetStatePropertyAll(primaryColor),
                  foregroundColor: WidgetStatePropertyAll(onPrimary),
                  textStyle: WidgetStatePropertyAll(
                    TextStyle(fontSize: 15, fontWeight: FontWeight(600)),
                  ),
                ),
                child: Text('Join now'),
              ),
              ElevatedButton(onPressed: null, child: Text('Sign in')),
            ],
          ),
        ),
      ),
      backgroundColor: backgroundColor,
    );
  }
}
