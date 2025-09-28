import 'package:flutter/material.dart';
import 'package:tes/models/event.dart';
import 'package:tes/utils/format_date.dart';
import 'package:tes/widgets/dialog_header.dart';
import 'package:tes/widgets/styled_dialog.dart';

class EventDialog extends StatelessWidget {
  final Event event;

  const EventDialog({super.key, required this.event});

  @override
  Widget build(BuildContext context) {
    return StyledDialog(
      header: DialogHeader(
        title: event.name,
        onClose: () => Navigator.pop(context),
      ),
      body: SingleChildScrollView(
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            Text(formatDate(event.year, event.month, event.day)),
            event.location != null
                ? Text(event.location!)
                : const SizedBox.shrink(),
            event.description != null
                ? Padding(
                    padding: const EdgeInsets.only(top: 8.0),
                    child: Text(event.description!),
                  )
                : const SizedBox.shrink(),
          ],
        ),
      ),
    );
  }
}
