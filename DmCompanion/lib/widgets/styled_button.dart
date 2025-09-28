import 'package:flutter/material.dart';

class StyledButton extends StatelessWidget {
  final String label;
  final VoidCallback onPressed;

  StyledButton({super.key, required this.label, required this.onPressed});

  @override
  Widget build(BuildContext context) {
    return Padding(
      padding: const EdgeInsets.symmetric(vertical: 8.0),
      child: ElevatedButton(
        onPressed: onPressed,
        child: Text(label),
      ),
    );
  }
}
