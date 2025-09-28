import 'package:firebase_auth/firebase_auth.dart';
import 'package:flutter/material.dart';
import 'package:tes/auth/auth_service.dart';
import 'package:tes/auth/login_page.dart';
import 'package:tes/services/ioc_container.dart';
import 'package:tes/widgets/handling_future_builder.dart';

class PageTemplate extends StatelessWidget {
  final String title;
  final Widget child;
  final AuthService _authService = get<AuthService>();

  PageTemplate({super.key, required this.title, required this.child});

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);

    return Scaffold(
        appBar: AppBar(
            title: Text(title),
            centerTitle: true,
            backgroundColor: theme.colorScheme.primary,
            foregroundColor: theme.colorScheme.onPrimary,
            actions: [
              HandlingFutureBuilder<User?>(
                  future: _authService.getCurrentUser(),
                  builder: (context, user) {
                    return Container(
                      constraints: BoxConstraints(
                          maxWidth: MediaQuery.of(context).size.width),
                      child: LayoutBuilder(builder: (context, constraints) {
                        if (constraints.maxWidth < 800) {
                          return buildSignOutButton(context);
                        }
                        return Row(
                          mainAxisSize: MainAxisSize.min,
                          children: [
                            Padding(
                              padding:
                                  const EdgeInsets.symmetric(horizontal: 8.0),
                              child: Center(
                                child: Text(
                                  user!.email!,
                                  style: TextStyle(fontSize: 14.0),
                                ),
                              ),
                            ),
                            buildSignOutButton(context),
                          ],
                        );
                      }),
                    );
                  })
            ]),
        body: SafeArea(
          child: child,
        ));
  }

  Widget buildSignOutButton(BuildContext context) {
    return IconButton(
      icon: Icon(Icons.logout),
      onPressed: () async {
        await _authService.signOut();
        Navigator.of(context).pushReplacement(
          MaterialPageRoute(
            builder: (context) => LoginPage(),
          ),
        );
      },
      tooltip: 'Sign Out',
    );
  }
}
