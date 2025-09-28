import 'package:flutter/material.dart';
import 'package:tes/models/action.dart';
import 'package:tes/models/battle.dart';
import 'package:tes/models/character.dart';
import 'package:tes/services/battle_service.dart';
import 'package:tes/services/character_service.dart';
import 'package:tes/services/ioc_container.dart';
import 'package:tes/widgets/battle/battle_order.dart';
import 'package:tes/widgets/character/character_avatar.dart';
import 'package:tes/widgets/handling_stream_builder.dart';
import 'package:tes/widgets/page_template.dart';

class BattlePage extends StatefulWidget {
  final String battleId;

  BattlePage({super.key, required this.battleId});

  @override
  State<BattlePage> createState() => _BattlePageState();
}

class _BattlePageState extends State<BattlePage> {
  final BattleService _battleService = get<BattleService>();
  final CharacterService _characterService = get<CharacterService>();
  int roundCount = 1;
  int turnCounter = 0;

  Future<void> _showParticipantDialog({
    required List<Character> participants,
    required ActionType actionType,
    required Character currentParticipant,
  }) async {
    TextEditingController valueController = TextEditingController();

    final target = await showDialog<Character>(
      context: context,
      builder: (context) => StatefulBuilder(
        builder: (context, setState) {
          return SimpleDialog(
            title: Text(actionType == ActionType.attack
                ? 'Select Target to Attack'
                : 'Select Target to Heal'),
            children: [
              Padding(
                padding: const EdgeInsets.all(16.0),
                child: TextField(
                  controller: valueController,
                  keyboardType: TextInputType.number,
                  decoration: InputDecoration(
                    labelText: actionType == ActionType.attack
                        ? 'Attack Damage'
                        : 'Heal Amount',
                    border: OutlineInputBorder(),
                  ),
                  onChanged: (value) {
                    setState(() {});
                  },
                ),
              ),
              ...participants.map((participant) {
                return SimpleDialogOption(
                  onPressed: valueController.text.isEmpty
                      ? null
                      : () {
                          Navigator.pop(context, participant);
                        },
                  child:
                      Text('${participant.name} - HP: ${participant.health}'),
                );
              }),
            ],
          );
        },
      ),
    );

    if (target != null && valueController.text.isNotEmpty) {
      int amount = int.tryParse(valueController.text) ?? 0;
      if (actionType == ActionType.attack) {
        await _characterService.attack(
            battleId: widget.battleId,
            attacker: currentParticipant,
            target: target,
            damage: amount,
            turn: turnCounter);
      } else if (actionType == ActionType.heal) {
        await _characterService.heal(
            battleId: widget.battleId,
            healer: currentParticipant,
            target: target,
            health: amount,
            turn: turnCounter);
      }
    }
  }

  @override
  Widget build(BuildContext context) {
    return HandlingStreamBuilder(
      stream: _battleService.streamById(widget.battleId),
      builder: (context, battle) {
        if (battle == null) {
          return PageTemplate(
            title: 'Battle not found',
            child: Center(
              child: Text('Battle not found'),
            ),
          );
        }

        return PageTemplate(
          title: battle.name,
          child: Padding(
            padding: const EdgeInsets.all(16.0),
            child: Column(
              children: [
                Expanded(
                  child: Row(
                    children: [
                      Expanded(
                        child: HandlingStreamBuilder(
                          stream: _characterService.streamByBattleId(battle.id),
                          builder: (context, participants) {
                            Character currentParticipant =
                                participants[turnCounter % participants.length];
                            return _buildMainColumn(participants,
                                currentParticipant, battle, context);
                          },
                        ),
                      ),
                    ],
                  ),
                ),
              ],
            ),
          ),
        );
      },
    );
  }

  Widget _buildMainColumn(
    List<Character> participants,
    Character currentParticipant,
    Battle battle,
    BuildContext context,
  ) {
    return LayoutBuilder(
      builder: (context, constraints) {
        return Column(
          mainAxisAlignment: MainAxisAlignment.spaceBetween,
          children: [
            if (constraints.maxHeight > 600)
              Flexible(
                child: BattleOrder(
                  players: participants,
                  orientation: Orientation.landscape,
                  currentTurnIndex: turnCounter,
                ),
              ),
            SizedBox(height: 16.0),
            Text(
              "Round: $roundCount",
              style: TextStyle(fontSize: 20),
            ),
            SizedBox(height: 8.0),
            Text(
              "Turn: ${currentParticipant.name}",
              style: TextStyle(fontSize: 16),
            ),
            SizedBox(height: 16.0),
            _buildControlButtons(participants, battle),
            SizedBox(height: 16.0),
            Flexible(
              flex: 2,
              child: CharacterAvatar(playerId: currentParticipant.id),
            ),
            SizedBox(height: 16.0),
            _buildPlayerActions(context, participants, currentParticipant),
          ],
        );
      },
    );
  }

  Widget _buildControlButtons(List<Character> participants, Battle battle) {
    return Row(
      mainAxisAlignment: MainAxisAlignment.center,
      children: [
        ElevatedButton.icon(
          onPressed: () {
            setState(() {
              turnCounter++;
              if (turnCounter % participants.length == 0) {
                roundCount++;
              }
            });
          },
          label: Text("Next Turn"),
          icon: Icon(Icons.arrow_forward),
        ),
        SizedBox(width: 16.0),
        ElevatedButton.icon(
          onPressed: () {
            _battleService.update(battle.copyWith(isFinished: true));
            Navigator.pop(context);
          },
          label: Text("End Battle"),
          icon: Icon(Icons.stop),
        ),
      ],
    );
  }

  Widget _buildPlayerActions(
    BuildContext context,
    List<Character> participants,
    Character currentParticipant,
  ) {
    return Row(
      mainAxisAlignment: MainAxisAlignment.center,
      children: [
        SizedBox(width: 16.0),
        ElevatedButton.icon(
          onPressed: () {
            _showParticipantDialog(
              participants: participants,
              actionType: ActionType.attack,
              currentParticipant: currentParticipant,
            );
          },
          label: Text("Attack"),
          icon: Icon(Icons.sports_mma),
        ),
        SizedBox(width: 16.0),
        ElevatedButton.icon(
          onPressed: () {
            _showParticipantDialog(
              participants: participants,
              actionType: ActionType.heal,
              currentParticipant: currentParticipant,
            );
          },
          label: Text("Heal"),
          icon: Icon(Icons.healing),
        ),
      ],
    );
  }
}
