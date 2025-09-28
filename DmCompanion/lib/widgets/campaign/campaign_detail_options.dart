import 'package:flutter/material.dart';
import 'package:material_symbols_icons/symbols.dart';
import 'package:tes/models/campaign.dart';
import 'package:tes/models/player.dart';
import 'package:tes/pages/battle_history_page.dart';
import 'package:tes/pages/players_page.dart';
import 'package:tes/widgets/battle/add_battle_dialog.dart';
import 'package:tes/widgets/event/add_event_dialog.dart';

class CampaignDetailOptions extends StatelessWidget {
  final bool showPlayerListButton;
  final List<Player> players;
  final Campaign campaign;

  CampaignDetailOptions({
    super.key,
    required this.showPlayerListButton,
    required this.players,
    required this.campaign,
  });

  @override
  Widget build(BuildContext context) {
    final showLabel = MediaQuery.of(context).size.width > 1000;

    return Row(
      mainAxisAlignment: MainAxisAlignment.spaceEvenly,
      children: _buildButtons(context: context, showLabel: showLabel),
    );
  }

  List<Widget> _buildButtons(
      {required BuildContext context, required bool showLabel}) {
    return [
      if (showPlayerListButton) ...[
        _buildButton(
          label: Text(
            'Show players',
            style: TextStyle(fontSize: 16.0),
          ),
          icon: Icon(Icons.group),
          showLabel: showLabel,
          onPressed: () {
            Navigator.of(context).push(
              MaterialPageRoute(
                builder: (context) => PlayersPage(
                  players: players,
                  campaignName: campaign.name,
                ),
              ),
            );
          },
        ),
        SizedBox(width: 8.0),
      ],
      _buildButton(
        label: Text(
          'Start a Battle',
          style: TextStyle(fontSize: 16.0),
        ),
        icon: Icon(Symbols.swords),
        showLabel: showLabel,
        onPressed: () async {
          _showAddDialog(
            context,
            AddBattleDialog(
              campaign: campaign,
            ),
          );
        },
      ),
      SizedBox(width: 8.0),
      _buildButton(
        label: Text("Go to Battle History"),
        icon: Icon(Symbols.history),
        showLabel: showLabel,
        onPressed: () {
          Navigator.push(
            context,
            MaterialPageRoute(
              builder: (context) => BattleHistoryPage(campaignId: campaign.id),
            ),
          );
        },
      ),
      SizedBox(width: 8.0),
      _buildButton(
        label: Text(
          'Add an Event',
          style: TextStyle(fontSize: 16.0),
        ),
        icon: Icon(Symbols.event),
        showLabel: showLabel,
        onPressed: () {
          _showAddDialog(
            context,
            AddEventDialog(
              campaign: campaign,
            ),
          );
        },
      )
    ];
  }

  Future<void> _showAddDialog(BuildContext context, Widget child) {
    return showDialog(
      context: context,
      builder: (context) {
        return StatefulBuilder(
          builder: (context, setDialogState) {
            return child;
          },
        );
      },
    );
  }

  Widget _buildButton(
      {required Widget label,
      required Icon icon,
      required bool showLabel,
      required Function() onPressed}) {
    return showLabel
        ? ElevatedButton.icon(
            icon: icon,
            onPressed: onPressed,
            label: Padding(
              padding: const EdgeInsets.all(16.0),
              child: label,
            ),
          )
        : IconButton.filled(
            onPressed: onPressed,
            icon: icon,
          );
  }
}
