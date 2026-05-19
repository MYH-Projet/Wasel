import 'package:flutter/material.dart';
import 'package:wasel/api/user_service.dart';
import 'package:wasel/main.dart';

class HomePage extends StatefulWidget {
  const HomePage({super.key});

  @override
  State<HomePage> createState() => _HomePageState();
}

class _HomePageState extends State<HomePage> {
  Map<String, dynamic>? _user;

  @override
  void initState() {
    super.initState();
    WidgetsBinding.instance.addPostFrameCallback((_) => _loadUser());
  }

  Future<void> _loadUser() async {
    final authService = InheritedAuth.of(context).authService;
    final user = await UserService.getUserInfo(authService);
    if (!context.mounted) return;
    setState(() => _user = user);
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      body: Center(
        child: _user == null
            ? const CircularProgressIndicator()
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
