import 'package:flutter/material.dart';
import 'package:tes/widgets/rounded_window.dart';

import '../../models/character.dart';
import '../character/character_avatar.dart';

class BattleOrder extends StatelessWidget {
  final Orientation orientation;
  final List<Character> players;
  final int currentTurnIndex;

  BattleOrder(
      {super.key,
      this.orientation = Orientation.portrait,
      required this.players,
      required this.currentTurnIndex});

  @override
  Widget build(BuildContext context) {
    return RoundedWindow(
      child: Padding(
        padding: const EdgeInsets.all(16.0),
        child: ListView.separated(
          scrollDirection: orientation == Orientation.landscape
              ? Axis.horizontal
              : Axis.vertical,
          itemCount: double.maxFinite.toInt(),
          // Here because flutter doesnt support infinite list
          itemBuilder: (context, index) {
            final player = players[(index + currentTurnIndex) % players.length];
            return CharacterAvatar(playerId: player.id);
          },
          separatorBuilder: (context, index) {
            return SizedBox(
              height: orientation == Orientation.portrait ? 16.0 : 0.0,
              width: orientation == Orientation.landscape ? 16.0 : 0.0,
            );
          },
        ),
      ),
    );
  }
}
