import 'package:flutter/material.dart';
import 'package:wasel/themes/colors.dart';

class WaselBottomBar extends StatefulWidget {
  const WaselBottomBar({super.key});

  @override
  State<WaselBottomBar> createState() => _BarState();
}

class _BarState extends State<WaselBottomBar> {
  @override
  Widget build(BuildContext context) {
    return BottomAppBar(
      color: surfaceColor,
      child: Row(
        mainAxisAlignment: MainAxisAlignment.spaceBetween,
        children: [
          SizedBox(width: 16),
          IconButton(onPressed: null, icon: const Icon(Icons.home)),
          IconButton(onPressed: null, icon: const Icon(Icons.request_page)),
          IconButton(onPressed: null, icon: const Icon(Icons.settings)),
          SizedBox(width: 16),
        ],
      ),
    );
  }
}
