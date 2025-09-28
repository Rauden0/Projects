import 'dart:async';

import 'package:flutter/material.dart';
import 'package:tes/models/campaign.dart';
import 'package:tes/models/event.dart';
import 'package:tes/services/event_service.dart';
import 'package:tes/services/ioc_container.dart';
import 'package:tes/widgets/event/event_list_tile.dart';
import 'package:tes/widgets/handling_stream_builder.dart';
import 'package:tes/widgets/rounded_window.dart';

const SEPARATOR_SIZE = 16.0;
const SEPARATOR_FONT_SIZE = 16.0;
const SEPARATOR_THICKNESS = 2.0;
const EVENT_WIDGET_WIDTH = 250.0;
const EVENT_WIDGET_HEIGHT = 100.0;

class EventList extends StatefulWidget {
  final Campaign campaign;
  final Axis scrollDirection;

  EventList({
    super.key,
    required this.campaign,
    required this.scrollDirection,
  });

  @override
  State<EventList> createState() => _EventListState();
}

class _EventListState extends State<EventList> {
  final _eventService = get<EventService>();
  StreamSubscription? _subscription;

  // the list will scroll to the now separator only when:
  // - the list is first built
  // - the list is updated with new events
  // - the time of the campaign changes
  bool? _wasScrolled;
  int? _eventsLength;
  int? _lastPastEventIndex;

  @override
  void initState() {
    super.initState();
    _subscription =
        _eventService.streamExtended(widget.campaign).listen((data) {
      if (mounted) {
        final (events, lastPastEventIndex) = data;
        if (_eventsLength != events.length ||
            _lastPastEventIndex != lastPastEventIndex) {
          setState(() {
            _wasScrolled = false;
            _eventsLength = events.length;
            _lastPastEventIndex = lastPastEventIndex;
          });
        }
      }
    });
  }

  @override
  void dispose() {
    _subscription?.cancel();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    return HandlingStreamBuilder(
      stream: _eventService.streamExtended(widget.campaign),
      builder: (context, data) {
        final (events, lastPastEventIndex) = data;

        return RoundedWindow(
          child: Padding(
            padding: const EdgeInsets.all(16.0),
            child: _buildListView(events, lastPastEventIndex, context),
          ),
        );
      },
    );
  }

  Widget _buildListView(
      List<Event> events, int lastPastEventIndex, BuildContext context) {
    if (events.isEmpty) {
      return Center(
        child: Text("No events"),
      );
    }

    // scroll to the now separator (only show one past event)
    final controller = ScrollController();
    WidgetsBinding.instance.addPostFrameCallback((_) async {
      if ((_wasScrolled == null || _wasScrolled == false) &&
          controller.hasClients &&
          lastPastEventIndex != -1) {
        final offset = lastPastEventIndex *
            (SEPARATOR_SIZE +
                (widget.scrollDirection == Axis.horizontal
                    ? EVENT_WIDGET_WIDTH
                    : EVENT_WIDGET_HEIGHT));
        await controller.animateTo(
          offset,
          duration: Duration(seconds: 1),
          curve: Curves.easeInOut,
        );
        setState(() {
          _wasScrolled = true;
        });
      }
    });

    return ListView.separated(
      controller: controller,
      scrollDirection: widget.scrollDirection,
      itemCount: events.length,
      itemBuilder: (context, index) {
        final event = events[index];
        final eventWidget = SizedBox(
          width: 250.0,
          height: 100.0,
          child: RoundedWindow(
            color: Theme.of(context).colorScheme.outline,
            backgroundColor: !event.wasSeen && index <= lastPastEventIndex
                ? Theme.of(context).colorScheme.secondary.withOpacity(0.1)
                : null,
            child: Center(
                child: EventListTile(
              event: event,
            )),
          ),
        );

        // if there are no past events and this is the first event,
        // add now separator before the event
        if (lastPastEventIndex == -1 && index == 0) {
          return switch (widget.scrollDirection) {
            Axis.horizontal => Row(
                crossAxisAlignment: CrossAxisAlignment.stretch,
                children: [
                  _buildNowSeparator(context),
                  eventWidget,
                ],
              ),
            Axis.vertical => Column(
                crossAxisAlignment: CrossAxisAlignment.stretch,
                children: [
                  _buildNowSeparator(context),
                  eventWidget,
                ],
              ),
          };
        }

        // if all events are past and this is the last event,
        // add now separator after the event
        if (lastPastEventIndex == events.length - 1 &&
            index == lastPastEventIndex) {
          return switch (widget.scrollDirection) {
            Axis.horizontal => Row(
                crossAxisAlignment: CrossAxisAlignment.stretch,
                children: [
                  eventWidget,
                  _buildNowSeparator(context),
                ],
              ),
            Axis.vertical => Column(
                crossAxisAlignment: CrossAxisAlignment.stretch,
                children: [
                  eventWidget,
                  _buildNowSeparator(context),
                ],
              ),
          };
        }

        return eventWidget;
      },
      separatorBuilder: (context, index) =>
          _separatorBuilder(context, index, lastPastEventIndex),
    );
  }

  Widget _separatorBuilder(
      BuildContext context, int index, int lastPastEventIndex) {
    if (index == lastPastEventIndex) {
      return _buildNowSeparator(context);
    }

    return _buildDefaultSeparator();
  }

  Widget _buildNowSeparator(BuildContext context) {
    final color = Theme.of(context).colorScheme.secondary;

    return Padding(
      padding: const EdgeInsets.all(SEPARATOR_SIZE),
      child: switch (widget.scrollDirection) {
        Axis.horizontal => _buildHorizontalNowSeparator(color),
        Axis.vertical => _buildVerticalNowSeparator(color),
      },
    );
  }

  Widget _buildHorizontalNowSeparator(Color color) {
    return Column(
      mainAxisAlignment: MainAxisAlignment.center,
      crossAxisAlignment: CrossAxisAlignment.center,
      children: [
        Flexible(
          child: VerticalDivider(
            thickness: SEPARATOR_THICKNESS,
            color: color,
          ),
        ),
        Padding(
          padding: const EdgeInsets.symmetric(vertical: 8.0),
          child: Text(
            "Now",
            style: TextStyle(fontSize: SEPARATOR_FONT_SIZE),
          ),
        ),
        Flexible(
          child: VerticalDivider(
            thickness: SEPARATOR_THICKNESS,
            color: color,
          ),
        ),
      ],
    );
  }

  Widget _buildVerticalNowSeparator(Color color) {
    return Row(
      mainAxisAlignment: MainAxisAlignment.center,
      crossAxisAlignment: CrossAxisAlignment.center,
      children: [
        Flexible(
          child: Divider(
            thickness: SEPARATOR_THICKNESS,
            color: color,
          ),
        ),
        Padding(
          padding: const EdgeInsets.symmetric(horizontal: SEPARATOR_SIZE),
          child: Text(
            "Now",
            style: TextStyle(fontSize: SEPARATOR_FONT_SIZE),
          ),
        ),
        Flexible(
          child: Divider(
            thickness: SEPARATOR_THICKNESS,
            color: color,
          ),
        ),
      ],
    );
  }

  Widget _buildDefaultSeparator() {
    return switch (widget.scrollDirection) {
      Axis.horizontal => SizedBox(width: SEPARATOR_SIZE),
      Axis.vertical => SizedBox(height: SEPARATOR_SIZE),
    };
  }
}
