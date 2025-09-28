import 'package:flutter/material.dart';
import 'package:tes/models/campaign.dart';
import 'package:tes/models/player.dart';
import 'package:tes/widgets/battle/choose_order_dialog.dart';
import 'package:tes/widgets/dialog_header.dart';
import 'package:tes/widgets/styled_dialog.dart';

import '../../models/enemy.dart';
import '../character/select_character_button.dart';

class AddBattleDialog extends StatefulWidget {
  final Campaign campaign;

  AddBattleDialog({super.key, required this.campaign});

  @override
  State<AddBattleDialog> createState() => _AddBattleDialogState();
}

class _AddBattleDialogState extends State<AddBattleDialog> {
  final _nameController = TextEditingController();
  final _locationController = TextEditingController();
  final _descriptionController = TextEditingController();

  final _selectedPlayers = <Player>[];
  final _selectedEnemies = <Enemy>[];

  void _updateState() {
    setState(() {});
  }

  @override
  void initState() {
    super.initState();
    _nameController.addListener(_updateState);
  }

  @override
  void dispose() {
    _nameController.removeListener(_updateState);
    _nameController.dispose();
    _locationController.dispose();
    _descriptionController.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    return StyledDialog(
      header: DialogHeader(
        title: 'Start a Battle',
        onClose: () => Navigator.pop(context),
      ),
      body: _buildForm(),
      footer: Row(
        mainAxisAlignment: MainAxisAlignment.end,
        children: [
          FilledButton.icon(
            icon: Icon(Icons.arrow_forward),
            onPressed: _nameController.text.isEmpty ||
                    _selectedPlayers.isEmpty ||
                    _selectedEnemies.isEmpty
                ? null
                : () {
                    showDialog(
                        context: context,
                        builder: (context) {
                          return ChooseOrderDialog(
                            campaign: widget.campaign,
                            battleName: _nameController.text,
                            battleDescription: _descriptionController.text,
                            battleLocation: _locationController.text,
                            selectedCharacters: [
                              ..._selectedPlayers,
                              ..._selectedEnemies,
                            ],
                          );
                        });
                  },
            label: Text("Continue"),
          ),
        ],
      ),
    );
  }

  Widget _buildForm() {
    return SingleChildScrollView(
      child: Column(
        children: [
          TextFormField(
            controller: _nameController,
            decoration: const InputDecoration(labelText: 'Name'),
          ),
          TextFormField(
            controller: _locationController,
            decoration: const InputDecoration(labelText: 'Location'),
          ),
          TextFormField(
            controller: _descriptionController,
            decoration: const InputDecoration(labelText: 'Description'),
            maxLines: 3,
          ),
          SizedBox(height: 16.0),
          SelectPlayersButton(
            campaignId: widget.campaign.id,
            selectedCharacters: _selectedPlayers,
            onCharacterSelected: (player) {
              setState(() {
                if (_selectedPlayers.contains(player)) {
                  _selectedPlayers.remove(player);
                  return;
                }
                if (_selectedPlayers.contains(player)) {
                  _selectedPlayers.remove(player);
                }
                _selectedPlayers.add(player as Player);
              });
            },
            isEnemy: false,
          ),
          SizedBox(height: 16.0),
          SelectPlayersButton(
            campaignId: widget.campaign.id,
            selectedCharacters: _selectedEnemies,
            onCharacterSelected: (enemy) {
              setState(() {
                if (_selectedEnemies.contains(enemy)) {
                  _selectedEnemies.remove(enemy);
                  return;
                }
                if (_selectedEnemies.contains(enemy)) {
                  _selectedEnemies.remove(enemy);
                }
                _selectedEnemies.add(enemy as Enemy);
              });
            },
            allowAdding: true,
            createNewPlayerLabel: 'Create New Enemy',
            label: 'Select Enemies',
            isEnemy: true,
          ),
        ],
      ),
    );
  }
}
