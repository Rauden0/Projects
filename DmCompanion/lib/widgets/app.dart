import 'package:flutter/material.dart';

import '../auth/auth_wrapper.dart';

class App extends StatelessWidget {
  const App({super.key});

  @override
  Widget build(BuildContext context) {
    return MaterialApp(
      theme: _buildThemeData(Brightness.light),
      darkTheme: _buildThemeData(Brightness.dark),
      home: AuthenticationWrapper(),
      debugShowCheckedModeBanner: false,
    );
  }

  ThemeData _buildThemeData(Brightness brightness) {
    return ThemeData(
      colorScheme: ColorScheme.fromSeed(
        seedColor: Colors.red,
        brightness: brightness,
      ),
    );
  }
}
