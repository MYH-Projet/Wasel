import 'package:flutter/material.dart';
import 'package:wasel/api/user.dart' as user;
import 'package:wasel/widgets/wasel_bottom_bar.dart';

class HomePage extends StatefulWidget {
  const HomePage({super.key});

  @override
  State<HomePage> createState() => _HomePageState();
}

class _HomePageState extends State<HomePage> {
  @override
  Widget build(BuildContext context) {
    return Scaffold(
      body: Center(child: Text('Logged in')),
      bottomNavigationBar: WaselBottomBar(),
    );
  }
}
