import 'package:flutter/material.dart';
import 'package:flutter_secure_storage/flutter_secure_storage.dart';
import 'package:flutter_appauth/flutter_appauth.dart';
import 'package:wasel/screens/client-screens/home_page.dart' as client;
import 'package:wasel/themes/colors.dart';
import 'package:wasel/themes/text_styles.dart';
import 'package:wasel/widgets/wasel_logo_horizontal.dart';

class WelcomeScreen extends StatefulWidget {
  const WelcomeScreen({super.key});

  @override
  State<WelcomeScreen> createState() => _AuthState();
}

class _AuthState extends State<WelcomeScreen> {
  late String accessToken;
  late String refreshToken;
  late String idToken;

  Future<bool> registerUser() async {
    return await authAction(['create']);
  }

  Future<bool> loginUser() async {
    return await authAction(['login']);
  }

  Future<bool> authAction(List<String> actions) async {
    FlutterAppAuth appAuth = FlutterAppAuth();

    final AuthorizationTokenResponse
    result = await appAuth.authorizeAndExchangeCode(
      AuthorizationTokenRequest(
        // Refactor to variables
        'wasel-mobile',
        // no idea what to name the callback, this will do for now hh
        'com.example.wasel://oauthredirect',
        discoveryUrl:
            'http://localhost:8000/auth/realms/wasel/.well-known/openid-configuration',
        promptValues: actions,
        scopes: ['openid', 'profile', 'email'],
        // this shouldn't be used in prod
        allowInsecureConnections: true,
      ),
    );

    setState(() {
      accessToken = result.accessToken!;
      // Technically this is not needed since our mobile app belongs to the same app provider, so we don't need the idToken to
      // authenticate the user and register him in our user stores and we only need the access token to speak to the api
      idToken = result.idToken!;

      refreshToken = result.refreshToken!;
    });

    print(refreshToken);

    await storeSensitiveTokens('access-token', accessToken);
    await storeSensitiveTokens('refresh-token', refreshToken);
    return true;
  }

  Future<void> storeSensitiveTokens(String name, String token) async {
    final storage = FlutterSecureStorage();

    await storage.write(key: name, value: token);
  }

  void navigateHome(BuildContext context) {
    if (context.mounted) {
      Navigator.push(
        context,
        MaterialPageRoute<void>(builder: (context) => const client.HomePage()),
      );
    }
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      body: Padding(
        padding: EdgeInsets.symmetric(horizontal: 32.0, vertical: 16),
        child: Align(
          alignment: Alignment.center,
          child: Column(
            spacing: 16,
            crossAxisAlignment: CrossAxisAlignment.stretch,
            children: [
              SizedBox(height: 16),
              WaselLogoHorizontal(),
              Image.asset('./assets/welcome-image.png'),
              Text(
                'Move anything in minutes',
                textAlign: TextAlign.center,
                style: displayText.copyWith(fontSize: 28),
              ),
              Opacity(
                opacity: 0.5,
                child: Text(
                  'Reliable, fast, and secure delivery across the city. Just tap and track.',
                  textAlign: TextAlign.center,
                  style: labelText,
                ),
              ),
              ElevatedButton(
                onPressed: () async {
                  try {
                    await registerUser();
                    navigateHome(context);
                  } on FlutterAppAuthPlatformException catch (e) {
                    print(e.details);
                  }
                },
                style: ButtonStyle(
                  shape: WidgetStatePropertyAll<RoundedRectangleBorder>(
                    RoundedRectangleBorder(
                      borderRadius: BorderRadius.all(Radius.circular(10)),
                    ),
                  ),
                  padding: WidgetStatePropertyAll(
                    EdgeInsets.symmetric(vertical: 12),
                  ),
                  backgroundColor: WidgetStatePropertyAll(primaryColor),
                  foregroundColor: WidgetStatePropertyAll(onPrimary),
                  textStyle: WidgetStatePropertyAll(bolderLabelText),
                ),
                child: Text('Join now'),
              ),
              ElevatedButton(
                onPressed: () async {
                  try {
                    await loginUser();
                    navigateHome(context);
                  } on FlutterAppAuthPlatformException catch (e) {
                    print(e.details);
                  }
                },
                style: ButtonStyle(
                  shape: WidgetStatePropertyAll<RoundedRectangleBorder>(
                    RoundedRectangleBorder(
                      borderRadius: BorderRadius.all(Radius.circular(10)),
                      side: BorderSide(color: primaryColorw600),
                    ),
                  ),
                  padding: WidgetStatePropertyAll(
                    EdgeInsets.symmetric(vertical: 12),
                  ),
                  backgroundColor: WidgetStatePropertyAll(surfaceColor),
                  foregroundColor: WidgetStatePropertyAll(onSurface),
                  textStyle: WidgetStatePropertyAll(bolderLabelText),
                ),
                child: Text('Sign in'),
              ),
            ],
          ),
        ),
      ),
      backgroundColor: backgroundColor,
    );
  }
}
