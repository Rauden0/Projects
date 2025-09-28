import 'package:flutter/material.dart';
import 'package:tes/models/character.dart';
import 'package:tes/widgets/character/character_avatar.dart';
import 'package:tes/widgets/rounded_window.dart';

class CharacterList extends StatelessWidget {
  final List<Character> players;
  final Orientation orientation;

  const CharacterList(
      {super.key,
      required this.players,
      this.orientation = Orientation.portrait});

  @override
  Widget build(BuildContext context) {
    return RoundedWindow(
      child: Padding(
        padding: const EdgeInsets.all(16.0),
        child: SizedBox(
          width: 250.0,
          child: ListView.separated(
            scrollDirection: orientation == Orientation.landscape
                ? Axis.horizontal
                : Axis.vertical,
            itemCount: players.length,
            itemBuilder: (context, index) {
              final player = players[index];
              return CharacterAvatar(playerId: player.id);
            },
            separatorBuilder: (context, index) {
              return SizedBox(height: 16.0);
            },
          ),
        ),
      ),
    );
  }
}
