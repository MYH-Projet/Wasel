import 'package:flutter/material.dart';
import 'package:mobile/widgets/wasel_logo.dart';
import 'package:mobile/themes/text_styles.dart';

class WaselLogoVertical extends StatelessWidget {
  const WaselLogoVertical({super.key});

  @override
  Widget build(BuildContext context) {
    return Column(
      crossAxisAlignment: CrossAxisAlignment.center,
      children: [
        WaselLogo(width: 128, height: 64),
        Text('Wasel', style: headingText),
      ],
    );
  }
}
