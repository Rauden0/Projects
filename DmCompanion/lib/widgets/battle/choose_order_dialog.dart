import 'package:flutter/material.dart';
import 'package:tes/models/battle.dart';
import 'package:tes/models/battle_player.dart';
import 'package:tes/models/campaign.dart';
import 'package:tes/models/character.dart';
import 'package:tes/models/enemy_battle.dart';
import 'package:tes/models/player.dart';
import 'package:tes/pages/battle_page.dart';
import 'package:tes/services/battle_player_service.dart';
import 'package:tes/services/battle_service.dart';
import 'package:tes/services/enemy_battle_service.dart';
import 'package:tes/services/ioc_container.dart';
import 'package:tes/widgets/dialog_header.dart';
import 'package:tes/widgets/styled_dialog.dart';

class ChooseOrderDialog extends StatefulWidget {
  final Campaign campaign;
  final String battleName;
  final String? battleDescription;
  final String? battleLocation;
  final List<Character> selectedCharacters;

  ChooseOrderDialog(
      {super.key,
      required this.campaign,
      required this.battleName,
      this.battleDescription,
      this.battleLocation,
      required this.selectedCharacters});

  @override
  State<ChooseOrderDialog> createState() => _ChooseOrderDialogState();
}

class _ChooseOrderDialogState extends State<ChooseOrderDialog> {
  final _battleService = get<BattleService>();
  final _battlePlayerService = get<BattlePlayerService>();
  final _enemyBattleService = get<EnemyBattleService>();

  late List<Character> _orderedCharacters;

  @override
  void initState() {
    super.initState();
    _orderedCharacters = List.from(widget.selectedCharacters);
  }

  @override
  Widget build(BuildContext context) {
    return StyledDialog(
      header: DialogHeader(
        title: 'Order Players',
        onClose: () => Navigator.pop(context),
      ),
      body: _buildForm(),
      footer: Row(
        mainAxisAlignment: MainAxisAlignment.end,
        children: [
          FilledButton.icon(
            icon: Icon(Icons.check),
            onPressed: () async {
              final battleId = await _addBattle(context);
              Navigator.pop(context);
              Navigator.of(context).push(
                MaterialPageRoute(
                  builder: (context) => BattlePage(
                    battleId: battleId,
                  ),
                ),
              );
            },
            label: Text("Start Battle"),
          ),
        ],
      ),
    );
  }

  Widget _buildForm() {
    return ReorderableListView(
      onReorder: (int oldIndex, int newIndex) {
        setState(() {
          if (newIndex > oldIndex) {
            newIndex -= 1;
          }
          final Character player = _orderedCharacters.removeAt(oldIndex);
          _orderedCharacters.insert(newIndex, player);
        });
      },
      children: [
        for (final player in _orderedCharacters)
          ListTile(
            key: ValueKey(player.id),
            title: Text(player.name),
            leading: Icon(Icons.drag_handle),
          ),
      ],
    );
  }

  Future<String> _addBattle(BuildContext context) async {
    final ref = await _battleService.add(
      Battle(
        id: '',
        campaignId: widget.campaign.id,
        name: widget.battleName,
        description: widget.battleDescription,
        location: widget.battleLocation,
        year: widget.campaign.year,
        month: widget.campaign.month,
        day: widget.campaign.day,
        wasSeen: true,
      ),
    );

    for (final entry in _orderedCharacters.asMap().entries) {
      final player = entry.value;
      final index = entry.key;
      if (player is Player) {
        await _battlePlayerService.add(
          BattlePlayer(
              id: "", battleId: ref.id, playerId: player.id, order: index),
        );
      } else {
        await _enemyBattleService.add(
          EnemyBattle(
              id: "", battleId: ref.id, enemyId: player.id, order: index),
        );
      }
    }
    Navigator.pop(context);
    return ref.id;
  }
}
