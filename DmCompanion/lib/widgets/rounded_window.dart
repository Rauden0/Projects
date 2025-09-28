import 'package:flutter/material.dart';

class RoundedWindow extends StatelessWidget {
  final Widget child;
  final Color? color;
  final Color? backgroundColor;

  const RoundedWindow(
      {super.key, required this.child, this.color, this.backgroundColor});

  @override
  Widget build(BuildContext context) {
    return DecoratedBox(
      decoration: BoxDecoration(
        border: Border.all(
          color: color ?? Theme.of(context).colorScheme.primary,
          width: 2.0,
        ),
        borderRadius: BorderRadius.circular(8.0),
        color: backgroundColor,
      ),
      child: child,
    );
  }
}
