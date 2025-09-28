import 'dart:math';

import 'package:flutter/material.dart';
import 'package:tes/enums/heart_icon.dart';
import 'package:tes/models/character.dart';
import 'package:tes/widgets/character/hearth_icon.dart';

class HealthBar extends StatelessWidget {
  final Character player;

  HealthBar({
    super.key,
    required this.player,
  });

  @override
  Widget build(BuildContext context) {
    return Container(
      width: 200,
      height: 40,
      decoration: BoxDecoration(
        borderRadius: BorderRadius.circular(8.0),
        border: Border.all(
          width: 2,
          color: Theme.of(context).colorScheme.onSecondary,
        ),
        color: Theme.of(context).colorScheme.onSecondary,
      ),
      child: Stack(
        children: [
          _buildCurrentHealth(),
          _buildCurrentHealthText(),
          Align(
            alignment: Alignment.centerLeft,
            child: HeathIcon(
              type: HearthIconType.plus,
              player: player,
            ),
          ),
          Align(
            alignment: Alignment.centerRight,
            child: HeathIcon(
              type: HearthIconType.minus,
              player: player,
            ),
          ),
        ],
      ),
    );
  }

  Widget _buildCurrentHealthText() {
    return Center(
      child: Text(
        '${player.health} / ${player.maxHealth}',
        style: TextStyle(
          fontSize: 16,
          fontWeight: FontWeight.bold,
        ),
      ),
    );
  }

  Widget _buildCurrentHealth() {
    return FractionallySizedBox(
      alignment: Alignment.centerLeft,
      widthFactor:
          (min(max(player.health, 0), player.maxHealth)) / player.maxHealth,
      child: Container(
        decoration: BoxDecoration(
          borderRadius: BorderRadius.circular(8.0),
          color: Colors.red.shade800,
        ),
      ),
    );
  }
}
