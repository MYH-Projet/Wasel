import 'package:flutter/material.dart';
import 'package:wasel/themes/text_styles.dart';

class ClientSettingsScreen extends StatelessWidget {
  const ClientSettingsScreen({super.key});

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      body: Center(child: Text('Settings', style: headingText)),
    );
  }
}
