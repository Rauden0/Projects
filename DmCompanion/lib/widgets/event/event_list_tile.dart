import 'package:flutter/material.dart';
import 'package:tes/models/event.dart';
import 'package:tes/services/event_service.dart';
import 'package:tes/services/ioc_container.dart';
import 'package:tes/utils/format_date.dart';
import 'package:tes/widgets/event/event_dialog.dart';

class EventListTile extends StatelessWidget {
  final Event event;
  final EventService _eventService = get<EventService>();

  EventListTile({super.key, required this.event});

  @override
  Widget build(BuildContext context) {
    return ListTile(
      title: Text(event.name),
      subtitle: Text(formatDate(event.year, event.month, event.day)),
      trailing: IconButton(
        onPressed: () {
          if (!event.wasSeen) {
            _eventService.update(event.copyWith(wasSeen: true));
          }
          showDialog(
            context: context,
            builder: (context) {
              return StatefulBuilder(
                builder: (context, setDialogState) {
                  return EventDialog(
                    event: event,
                  );
                },
              );
            },
          );
        },
        icon: Icon(
          Icons.info_outline,
          color: Theme.of(context).colorScheme.secondary,
        ),
      ),
    );
  }
}
