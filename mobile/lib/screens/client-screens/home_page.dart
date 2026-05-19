import 'package:flutter/material.dart';
import 'package:wasel/api/user_service.dart';
import 'package:wasel/main.dart';
import 'package:wasel/screens/welcome_screen.dart';

class HomePage extends StatefulWidget {
  const HomePage({super.key});

  @override
  State<HomePage> createState() => _HomePageState();
}

class _HomePageState extends State<HomePage> {
  Map<String, dynamic>? _user;
  bool _loading = true;

  @override
  void initState() {
    super.initState();
    WidgetsBinding.instance.addPostFrameCallback((_) => _loadUser());
  }

  Future<void> _loadUser() async {
    final authService = InheritedAuth.of(context).authService;
    final result = await UserService.getUserInfo(authService);

    if (!context.mounted) return;

    if (result.isSuccess) {
      setState(() {
        _user = result.data;
        _loading = false;
      });
    } else {
      setState(() => _loading = false);

      final message = switch (result.error) {
        UserServiceError.unauthorized =>
          'Session expired, please sign in again',
        UserServiceError.network =>
          'Could not reach the server, please check your connection',
        UserServiceError.server => 'Something went wrong, please try again',
        null => 'Something went wrong',
      };

      ScaffoldMessenger.of(context).showSnackBar(
        SnackBar(content: Text(message), duration: const Duration(seconds: 3)),
      );

      if (result.error == UserServiceError.unauthorized) {
        await InheritedAuth.of(context).authService.clearTokens();
        if (!context.mounted) return;
        Navigator.pushReplacement(
          context,
          MaterialPageRoute(builder: (context) => const WelcomeScreen()),
        );
      }
    }
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      body: Center(
        child: _loading
            ? const CircularProgressIndicator()
            : _user == null
            ? const Text('Could not load profile')
            : Column(
                crossAxisAlignment: CrossAxisAlignment.stretch,
                mainAxisAlignment: MainAxisAlignment.center,
                children: [
                  Center(child: Text('Name: ${_user!['name']}')),
                  Center(child: Text('Email: ${_user!['email']}')),
                ],
              ),
      ),
    );
  }
}
