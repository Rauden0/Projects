import 'dart:io';
import 'package:flutter/foundation.dart';
import 'package:flutter/material.dart';
import 'package:tes/generated/assets.gen.dart';
import 'package:tes/models/campaign_player.dart';
import 'package:tes/models/enemy.dart';
import 'package:tes/models/player.dart';
import 'package:tes/services/campaign_player_service.dart';
import 'package:tes/services/enemy_service.dart';
import 'package:tes/services/ioc_container.dart';
import 'package:tes/services/player_service.dart';
import 'package:tes/widgets/form_page.dart';
import 'package:tes/widgets/styled_dialog.dart';

class AddPlayerPage extends StatefulWidget {
  final String? label;
  final String? campaignId;
  final bool isEnemy;

  AddPlayerPage({super.key, this.label, this.campaignId, this.isEnemy = false});

  @override
  State<AddPlayerPage> createState() => _AddPlayerPageState();
}

class _AddPlayerPageState extends State<AddPlayerPage> {
  final TextEditingController _playerNameController = TextEditingController();
  final TextEditingController _maxHealthController =
      TextEditingController(text: '100');

  String? playerNameError;
  String? maxHealthError;

  File? _image;
  AssetGenImage? _avatar = Assets.images.playerAvatars.placeholder;
  Uint8List? _imageBytes;

  final _playerService = get<PlayerService>();
  final _enemyService = get<EnemyService>();
  final _campaignPlayerService = get<CampaignPlayerService>();

  @override
  Widget build(BuildContext context) {
    return FormPage(
      title: widget.label ?? 'Create New Player',
      children: [
        Text(widget.label ?? 'Create New Player',
            style: TextStyle(fontSize: 24)),
        _buildTextField(
          controller: _playerNameController,
          labelText: 'Name',
          errorText: playerNameError,
          onChanged: (value) {
            setState(() {
              playerNameError = value.isEmpty ? 'Name is required' : null;
            });
          },
        ),
        _buildTextField(
          controller: _maxHealthController,
          labelText: 'Max Health',
          errorText: maxHealthError,
          keyboardType: TextInputType.number,
          onChanged: (value) {
            setState(() {
              maxHealthError = _validateMaxHealth(value);
            });
          },
        ),
        ElevatedButton(
          onPressed: _showImageSelectDialog,
          child: const Text('Select Avatar'),
        ),
        if (_avatar != null || _image != null || _imageBytes != null)
          _buildImagePreview(),
        _buildAddPlayerButton(),
      ],
    );
  }

  Widget _buildTextField({
    required TextEditingController controller,
    required String labelText,
    String? errorText,
    TextInputType keyboardType = TextInputType.text,
    required ValueChanged<String> onChanged,
  }) {
    return TextField(
      controller: controller,
      keyboardType: keyboardType,
      decoration: InputDecoration(
        labelText: labelText,
        errorText: errorText,
        border: const OutlineInputBorder(),
      ),
      onChanged: onChanged,
    );
  }

  String? _validateMaxHealth(String value) {
    if (value.isEmpty || int.tryParse(value) == null || int.parse(value) <= 0) {
      return 'Enter a valid positive number';
    }
    return null;
  }

  Widget _buildImagePreview() {
    return Padding(
      padding: const EdgeInsets.all(8.0),
      child: Center(
        child: _avatar != null
            ? _avatar!.image(height: 200, width: 200, fit: BoxFit.cover)
            : kIsWeb
                ? Image.memory(_imageBytes!,
                    height: 200, width: 200, fit: BoxFit.cover)
                : Image.file(_image!,
                    height: 200, width: 200, fit: BoxFit.cover),
      ),
    );
  }

  Widget _buildAddPlayerButton() {
    return ElevatedButton.icon(
      icon: const Icon(Icons.add),
      label: Padding(
        padding: const EdgeInsets.all(12.0),
        child: Text(widget.label ?? 'Create New Player',
            style: TextStyle(fontSize: 16.0)),
      ),
      onPressed: _onAddPlayer,
    );
  }

  void _showImageSelectDialog() {
    showDialog(
      context: context,
      builder: (context) {
        return StyledDialog(
          body: SingleChildScrollView(
            child: Column(
              mainAxisSize: MainAxisSize.min,
              children: [
                const Text('Select Player Avatar',
                    style: TextStyle(fontSize: 18)),
                const SizedBox(height: 16.0),
                ...Assets.images.playerAvatars.values.map((avatar) {
                  return ListTile(
                    leading:
                        avatar.image(width: 50, height: 50, fit: BoxFit.cover),
                    title: Text(avatar.path.split('/').last),
                    onTap: () {
                      setState(() {
                        _image = null;
                        _imageBytes = null;
                        _avatar = avatar;
                      });
                      Navigator.pop(context);
                    },
                  );
                }),
              ],
            ),
          ),
        );
      },
    );
  }

  Future<void> _onAddPlayer() async {
    if (!_validateInputs()) return;
    final newCharacter = widget.isEnemy
        ? Enemy(
            id: "",
            name: _playerNameController.text.trim(),
            maxHealth: int.parse(_maxHealthController.text.trim()),
            imagePath: _avatar!.path,
          )
        : Player(
            id: "",
            name: _playerNameController.text.trim(),
            maxHealth: int.parse(_maxHealthController.text.trim()),
            imagePath: _avatar!.path,
          );

    final ref = widget.isEnemy
        ? await _enemyService.add(newCharacter as Enemy)
        : await _playerService.add(newCharacter as Player);

    if (widget.campaignId != null) {
      _campaignPlayerService.add(
        CampaignPlayer(
          id: "",
          campaignId: widget.campaignId!,
          playerId: ref.id,
          isEnemy: widget.isEnemy,
        ),
      );
    }

    if (mounted) Navigator.pop(context);
  }

  bool _validateInputs() {
    setState(() {
      playerNameError =
          _playerNameController.text.isEmpty ? 'Player name is required' : null;
      maxHealthError = _validateMaxHealth(_maxHealthController.text);
    });
    return playerNameError == null && maxHealthError == null;
  }
}
