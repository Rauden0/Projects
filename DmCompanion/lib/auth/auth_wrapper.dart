import 'package:firebase_auth/firebase_auth.dart';
import 'package:flutter/material.dart';
import 'package:tes/auth/auth_service.dart';
import 'package:tes/pages/campaigns_page.dart';
import 'package:tes/services/ioc_container.dart';
import 'package:tes/widgets/handling_stream_builder.dart';
import 'login_page.dart';

class AuthenticationWrapper extends StatelessWidget {
  const AuthenticationWrapper({super.key});

  @override
  Widget build(BuildContext context) {
    final authService = get<AuthService>();
    return HandlingStreamBuilder<User?>(
      allowNull: true,
      stream: authService.getCurrentUserStream(),
      builder: (context, user) {
        if (user != null) {
          return CampaignsPage();
        } else {
          return LoginPage();
        }
      },
    );
  }
}
