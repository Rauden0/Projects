import 'package:flutter/material.dart';

class StyledAlert extends StatelessWidget {
  final String title;
  final List<String> content;
  final String confirmTitle;
  final Icon confirmIcon;
  final VoidCallback onConfirm;

  const StyledAlert(
      {super.key,
      required this.title,
      required this.content,
      required this.confirmTitle,
      required this.onConfirm,
      required this.confirmIcon});

  @override
  Widget build(BuildContext context) {
    return AlertDialog(
      title: Text(title),
      content: SingleChildScrollView(
        child: ListBody(
          children: content.map((String text) => Text(text)).toList(),
        ),
      ),
      actions: <Widget>[
        TextButton(
          onPressed: () {
            Navigator.of(context).pop();
          },
          child: const Text('Cancel'),
        ),
        FilledButton.icon(
          icon: confirmIcon,
          onPressed: () {
            onConfirm();
            Navigator.of(context).pop();
          },
          label: Text(confirmTitle),
        ),
      ],
    );
  }
}
