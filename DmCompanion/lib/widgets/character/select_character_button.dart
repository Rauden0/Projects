import 'package:flutter/material.dart';
import 'package:tes/models/character.dart';
import 'package:tes/pages/add_player_page.dart';
import 'package:tes/services/enemy_service.dart';
import 'package:tes/services/ioc_container.dart';
import 'package:tes/services/player_service.dart';
import 'package:tes/widgets/dialog_header.dart';
import 'package:tes/widgets/handling_stream_builder.dart';
import 'package:tes/widgets/styled_dialog.dart';

class SelectPlayersButton extends StatefulWidget {
  final bool allowAdding;
  final VoidCallback? onDone;
  final void Function(Character character) onCharacterSelected;
  final List<Character> selectedCharacters;
  final String? campaignId;
  final String? label;
  final String? createNewPlayerLabel;
  final bool isEnemy;

  const SelectPlayersButton({
    super.key,
    this.allowAdding = false,
    this.onDone,
    this.campaignId,
    required this.onCharacterSelected,
    required this.selectedCharacters,
    this.label,
    this.createNewPlayerLabel,
    this.isEnemy = false,
  });

  @override
  State<SelectPlayersButton> createState() => _SelectPlayersButtonState();
}

class _SelectPlayersButtonState extends State<SelectPlayersButton> {
  final _dialogFilterController = TextEditingController();
  late final PlayerService _playerService;
  late final EnemyService _enemyService;

  @override
  void initState() {
    super.initState();
    _playerService = get<PlayerService>();
    _enemyService = get<EnemyService>();
  }

  @override
  Widget build(BuildContext context) {
    return ElevatedButton.icon(
      icon: const Icon(Icons.group),
      onPressed: () {
        _showSelectPlayersDialog(context);
      },
      label: Padding(
        padding: EdgeInsets.all(12.0),
        child: Text(
          widget.label ?? 'Select Players',
          style: TextStyle(fontSize: 16.0),
        ),
      ),
    );
  }

  void _showSelectPlayersDialog(BuildContext context) {
    showDialog(
      context: context,
      builder: (context) {
        return StatefulBuilder(
          builder: (context, setDialogState) {
            return _buildSelectPlayersDialog(
              context,
              setDialogState,
            );
          },
        );
      },
    );
  }

  Widget _buildSelectPlayersDialog(
      BuildContext context, StateSetter setDialogState) {
    return StyledDialog(
      header: DialogHeader(
        title: widget.label ?? "Select Characters",
        onClose: () => Navigator.pop(context),
      ),
      body: Column(
        mainAxisSize: MainAxisSize.min,
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          _buildDialogFilterField(setDialogState),
          const SizedBox(height: 16.0),
          _buildCharacterList(setDialogState),
          widget.allowAdding ? SizedBox(height: 16.0) : const SizedBox(),
          widget.allowAdding ? _buildAddNewCharacterField() : const SizedBox(),
        ],
      ),
      footer: Row(
        mainAxisAlignment: MainAxisAlignment.end,
        children: [
          FilledButton.icon(
            icon: Icon(Icons.check),
            onPressed: widget.selectedCharacters.isEmpty
                ? null
                : () {
                    if (widget.onDone != null) {
                      widget.onDone!();
                    }
                    Navigator.pop(context);
                  },
            label: Text("Select"),
          ),
        ],
      ),
    );
  }

  Widget _buildDialogFilterField(StateSetter setDialogState) {
    return Column(
      crossAxisAlignment: CrossAxisAlignment.start,
      children: [
        const SizedBox(height: 8.0),
        TextField(
          controller: _dialogFilterController,
          decoration: const InputDecoration(
            labelText: 'Filter by name',
            border: OutlineInputBorder(),
          ),
          onChanged: (value) {
            setDialogState(() {});
          },
        ),
      ],
    );
  }

  Widget _buildCharacterList(StateSetter setDialogState) {
    return Flexible(
      child: HandlingStreamBuilder<List<Character>>(
        stream: widget.isEnemy
            ? _enemyService.stream
            : widget.campaignId == null
                ? _playerService.stream
                : _playerService.streamByCampaignIdFiltered(widget.campaignId!),
        builder: (context, characters) {
          final filteredCharacters = characters
              .where((character) => character.name
                  .toLowerCase()
                  .contains(_dialogFilterController.text.toLowerCase()))
              .toList();

          return ListView.builder(
            itemCount: filteredCharacters.length,
            itemBuilder: (context, index) {
              final character = filteredCharacters[index];
              final isSelected = widget.selectedCharacters.contains(character);
              return ListTile(
                title: Text(character.name),
                trailing: isSelected
                    ? const Icon(Icons.check, color: Colors.green)
                    : null,
                onTap: () =>
                    _toggleCharacterSelection(character, setDialogState),
              );
            },
          );
        },
      ),
    );
  }

  Widget _buildAddNewCharacterField() {
    return Column(
      crossAxisAlignment: CrossAxisAlignment.start,
      children: [
        const SizedBox(height: 16.0),
        ElevatedButton(
          onPressed: () async {
            await Navigator.push<Character?>(
              context,
              MaterialPageRoute(
                builder: (context) => AddPlayerPage(
                  label: widget.createNewPlayerLabel,
                  campaignId: widget.campaignId,
                  isEnemy: widget.isEnemy,
                ),
              ),
            );
          },
          child: Text(widget.createNewPlayerLabel ?? 'Create New Character'),
        ),
      ],
    );
  }

  void _toggleCharacterSelection(
      Character character, StateSetter setDialogState) {
    widget.onCharacterSelected(character);
    setDialogState(() {});
  }
}
