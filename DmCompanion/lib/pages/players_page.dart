import 'package:flutter/material.dart';
import 'package:tes/models/player.dart';
import 'package:tes/widgets/page_template.dart';

import '../widgets/character/character_list.dart';

class PlayersPage extends StatelessWidget {
  final String campaignName;
  final List<Player> players;

  const PlayersPage(
      {super.key, required this.players, required this.campaignName});

  @override
  Widget build(BuildContext context) {
    return PageTemplate(
      title: campaignName,
      child: Padding(
          padding: const EdgeInsets.all(16.0),
          child: Center(child: CharacterList(players: players))),
    );
  }
}
