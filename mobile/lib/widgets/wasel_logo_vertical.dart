import 'package:flutter/material.dart';
import 'package:mobile/widgets/wasel_logo.dart';

class WaselLogoVertical extends StatelessWidget {
  const WaselLogoVertical({super.key});

  @override
  Widget build(BuildContext context) {
    return const Column(
      children: [
        WaselLogo(width: 128, height: 64),
        Text(
          'Wasel',
          style: TextStyle(fontWeight: FontWeight(700), fontSize: 24),
        ),
      ],
    );
  }
}
