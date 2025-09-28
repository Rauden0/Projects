import 'package:flutter/material.dart';

class StyledDialog extends StatelessWidget {
  final Widget? header;
  final Widget body;
  final Widget? footer;

  const StyledDialog({
    super.key,
    this.header,
    required this.body,
    this.footer,
  });

  @override
  Widget build(BuildContext context) {
    return Dialog(
      shape: RoundedRectangleBorder(
        borderRadius: BorderRadius.circular(16.0),
      ),
      child: Container(
        padding: const EdgeInsets.all(16.0),
        constraints: const BoxConstraints(maxWidth: 400, maxHeight: 500),
        child: Column(
          mainAxisSize: MainAxisSize.min,
          crossAxisAlignment: CrossAxisAlignment.stretch,
          children: [
            if (header != null) header!,
            if (header != null) const SizedBox(height: 16.0),
            Flexible(child: body),
            if (footer != null) const SizedBox(height: 16.0),
            if (footer != null) footer!,
          ],
        ),
      ),
    );
  }
}
