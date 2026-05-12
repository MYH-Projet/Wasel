import 'package:flutter/material.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:mobile/main.dart';

void main() {
  testWidgets('App displays Hello World!', (WidgetTester tester) async {
    // Build the app
    await tester.pumpWidget(const MainApp());

    // Verify "Hello World!" is displayed
    expect(find.text('Hello World!'), findsOneWidget);
  });
}
