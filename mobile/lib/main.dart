import 'package:flutter/material.dart';
import 'package:wasel/api/auth_service.dart';
import 'package:wasel/screens/splash_screen.dart';

void main() {
  runApp(const MainApp());
}

class MainApp extends StatelessWidget {
  const MainApp({super.key});

  @override
  Widget build(BuildContext context) {
    return InheritedAuth(
      authService: AuthService(),
      child: const MaterialApp(home: SplashScreen()),
    );
  }
}

class InheritedAuth extends InheritedWidget {
  final AuthService authService;

  const InheritedAuth({
    super.key,
    required this.authService,
    required super.child,
  });

  static InheritedAuth of(BuildContext context) {
    return context.dependOnInheritedWidgetOfExactType<InheritedAuth>()!;
  }

  @override
  bool updateShouldNotify(InheritedAuth oldWidget) => false;
}
