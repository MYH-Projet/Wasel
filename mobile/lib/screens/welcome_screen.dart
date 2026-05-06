import 'package:flutter/material.dart';
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
                margin: EdgeInsets.all(24.0),
                child: WaselLogoVertical(),
              ),
            ],
          ),
        ),
      ),
      backgroundColor: Color.fromARGB(255, 255, 255, 255),
    );
  }
}
