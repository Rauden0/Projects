import 'package:flutter/material.dart';
import 'package:flutter/services.dart';
import 'package:popover/popover.dart';
import 'package:tes/enums/heart_icon.dart';
import 'package:tes/services/ioc_container.dart';
import 'package:tes/services/player_service.dart';

import '../../models/character.dart';

class HeathIcon extends StatefulWidget {
  final _playerService = get<PlayerService>();

  final HearthIconType type;
  final Character player;

  HeathIcon({super.key, required this.type, required this.player});

  @override
  State<HeathIcon> createState() => _HeathIconState();
}

class _HeathIconState extends State<HeathIcon> {
  final TextEditingController _controller = TextEditingController();

  @override
  Widget build(BuildContext context) {
    return Align(
        alignment: widget.type.alignment,
        child: IconButton(
          icon: widget.type.icon,
          onPressed: () {
            showPopover(
              context: context,
              bodyBuilder: (context) => Padding(
                padding: const EdgeInsets.all(16.0),
                child: SizedBox(
                  width: 120,
                  child: TextField(
                    controller: _controller,
                    decoration: InputDecoration(labelText: widget.type.label),
                    keyboardType: TextInputType.number,
                    inputFormatters: <TextInputFormatter>[
                      FilteringTextInputFormatter.digitsOnly
                    ],
                    onSubmitted: (value) => {
                      widget._playerService.changeHealth(
                          player: widget.player,
                          delta: widget.type.delta(int.parse(value))),
                      _controller.clear(),
                      Navigator.of(context).pop()
                    },
                  ),
                ),
              ),
              direction: widget.type.direction,
              backgroundColor: Theme.of(context).colorScheme.surface,
            );
          },
        ));
  }
}
