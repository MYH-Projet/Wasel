import 'package:flutter/material.dart';
import 'package:wasel/themes/text_styles.dart';

class DriverRequestsScreen extends StatelessWidget {
  const DriverRequestsScreen({super.key});

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      body: Center(child: Text('Requests', style: headingText)),
    );
  }
}
