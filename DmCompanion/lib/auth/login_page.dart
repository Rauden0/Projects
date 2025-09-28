import 'package:firebase_auth/firebase_auth.dart';
import 'package:flutter/material.dart';
import 'package:tes/auth/auth_service.dart';
import 'package:tes/pages/campaigns_page.dart';
import 'package:tes/services/ioc_container.dart';
import 'package:tes/widgets/page_template.dart';

const SPACER_SIZE = 16.0;

class LoginPage extends StatefulWidget {
  const LoginPage({super.key});

  @override
  State<LoginPage> createState() => _LoginPageState();
}

class _LoginPageState extends State<LoginPage> {
  final TextEditingController emailController = TextEditingController();
  final TextEditingController passwordController = TextEditingController();
  final AuthService _authService = get<AuthService>();
  String? errorMessage;
  bool isLoading = false;
  @override
  void dispose() {
    emailController.dispose();
    passwordController.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    return PageTemplate(
      title: "DM Companion",
      child: Padding(
        padding: const EdgeInsets.all(40.0),
        child: Center(
          child: _buildLoginForm(),
        ),
      ),
    );
  }

  Widget _buildLoginForm() {
    return SizedBox(
      width: 600,
      child: Column(
        mainAxisAlignment: MainAxisAlignment.center,
        crossAxisAlignment: CrossAxisAlignment.stretch,
        children: [
          Text(
            "Welcome!",
            textAlign: TextAlign.center,
            style: const TextStyle(fontSize: 24, fontWeight: FontWeight.bold),
          ),
          const SizedBox(height: 2 * SPACER_SIZE),
          _buildTextField(
            controller: emailController,
            labelText: "Email",
            keyboardType: TextInputType.emailAddress,
          ),
          const SizedBox(height: SPACER_SIZE),
          _buildTextField(
            controller: passwordController,
            labelText: "Password",
            obscureText: true,
          ),
          if (errorMessage != null) ...[
            const SizedBox(height: SPACER_SIZE),
            Text(
              errorMessage!,
              style: const TextStyle(color: Colors.red),
              textAlign: TextAlign.center,
            ),
          ],
          const SizedBox(height: 2 * SPACER_SIZE),
          Row(
            mainAxisAlignment: MainAxisAlignment.center,
            children: [
              _buildLoginButton(),
              const SizedBox(width: SPACER_SIZE),
              _buildSignUpButton(),
            ],
          ),
        ],
      ),
    );
  }

  Widget _buildTextField({
    required TextEditingController controller,
    required String labelText,
    bool obscureText = false,
    TextInputType? keyboardType,
  }) {
    return TextField(
      controller: controller,
      obscureText: obscureText,
      keyboardType: keyboardType,
      decoration: InputDecoration(
        labelText: labelText,
        border: const OutlineInputBorder(),
      ),
    );
  }

  Widget _buildLoginButton() {
    return OutlinedButton.icon(
      icon: Icon(Icons.login),
      onPressed: isLoading
          ? null
          : () async {
              final email = emailController.text.trim();
              final password = passwordController.text.trim();
              if (email.isEmpty || password.isEmpty) {
                setState(() {
                  errorMessage = "Please fill in both email and password.";
                });
                return;
              }

              setState(() {
                isLoading = true;
                errorMessage = null;
              });

              try {
                User? user = await _authService.signIn(email, password);
                if (!mounted) return;
                if (user == null) {
                  setState(() {
                    errorMessage =
                        "Invalid email or password. Please try again.";
                    isLoading = false;
                  });
                  return;
                }
                Navigator.pushReplacement(
                  context,
                  MaterialPageRoute(
                    builder: (context) => CampaignsPage(),
                  ),
                );
              } catch (e) {
                setState(() {
                  errorMessage = "An error occurred. Please try again.";
                  isLoading = false;
                });
              }
            },
      label: Padding(
        padding: const EdgeInsets.all(16.0),
        child: isLoading
            ? const CircularProgressIndicator(color: Colors.white)
            : const Text("Sign In", style: TextStyle(fontSize: 16)),
      ),
    );
  }

  Widget _buildSignUpButton() {
    return OutlinedButton.icon(
      icon: Icon(Icons.person_add),
      onPressed: isLoading
          ? null
          : () async {
              final email = emailController.text.trim();
              final password = passwordController.text.trim();
              if (email.isEmpty || password.isEmpty) {
                setState(() {
                  errorMessage = "Please fill in both email and password.";
                });
                return;
              }

              setState(() {
                isLoading = true;
                errorMessage = null;
              });

              try {
                User? user = await _authService.signUp(email, password);
                if (!mounted) return;
                if (user == null) {
                  setState(() {
                    errorMessage = "Unable to sign up. Please try again.";
                    isLoading = false;
                  });
                  return;
                }
                Navigator.pushReplacement(
                  context,
                  MaterialPageRoute(
                    builder: (context) => CampaignsPage(),
                  ),
                );
              } catch (e) {
                setState(() {
                  errorMessage = "Unable to sign up. Please try again.";
                  isLoading = false;
                });
              }
            },
      label: Padding(
        padding: const EdgeInsets.all(16.0),
        child: isLoading
            ? const CircularProgressIndicator()
            : const Text("Sign Up", style: TextStyle(fontSize: 16)),
      ),
    );
  }
}
