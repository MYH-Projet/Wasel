import 'package:flutter/material.dart';
import 'package:mobile/widgets/wasel_logo.dart';

class WaselLogoHorizontal extends StatelessWidget {
  const WaselLogoHorizontal({super.key});

  @override
  Widget build(BuildContext context) {
    return const Row(
      mainAxisAlignment: MainAxisAlignment.center,
      children: [
        WaselLogo(width: 64, height: 64),
        Text(
          'Wasel',
          style: TextStyle(fontWeight: FontWeight(700), fontSize: 24),
        ),
      ],
    );
  }
}
