import 'package:flutter/material.dart';
import 'package:mobile/widgets/wasel_logo_horizontal.dart';
import 'package:mobile/widgets/wasel_logo_vertical.dart';

class WelcomeScreen extends StatelessWidget {
  const WelcomeScreen({required this.userName, super.key});

  final String userName;

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      body: Padding(
        padding: EdgeInsets.all(16.0),
        child: Align(
          alignment: Alignment.center,
          child: Column(
            children: [
              Container(
                margin: EdgeInsets.symmetric(horizontal: 24.0, vertical: 0),
                child: WaselLogoHorizontal(),
              ),
              Image.asset('./assets/welcome-image.png'),
              Text('Move anything in minutes'),
              Text(
                'Reliable, fast, and secure delivery across the city. Just tap and track.',
              ),
              ElevatedButton(
                onPressed: null,
                style: ButtonStyle(
                  backgroundColor: WidgetStatePropertyAll(
                    Color.fromARGB(255, 247, 203, 21),
                  ),
                  foregroundColor: WidgetStatePropertyAll(
                    Color.fromARGB(255, 44, 62, 80),
                  ),
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
      backgroundColor: Color.fromARGB(255, 255, 255, 255),
    );
  }
}
