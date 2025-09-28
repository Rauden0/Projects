import 'package:flutter/material.dart';
import 'package:tes/models/campaign.dart';
import 'package:tes/models/campaign_player.dart';
import 'package:tes/models/player.dart';
import 'package:tes/services/campaign_player_service.dart';
import 'package:tes/services/campaign_service.dart';
import 'package:tes/services/ioc_container.dart';
import 'package:tes/widgets/form_page.dart';

import '../widgets/character/select_character_button.dart';

class CreateNewCampaignPage extends StatefulWidget {
  CreateNewCampaignPage({super.key});

  final _campaignService = get<CampaignService>();
  final _campaignPlayerService = get<CampaignPlayerService>();

  @override
  State<CreateNewCampaignPage> createState() => _CreateNewCampaignPageState();
}

class _CreateNewCampaignPageState extends State<CreateNewCampaignPage> {
  final campaignNameController = TextEditingController();
  final campaignDescriptionController = TextEditingController();
  final dialogFilterController = TextEditingController();
  String? campaignNameError;
  List<Player> selectedPlayers = [];

  @override
  Widget build(BuildContext context) {
    return FormPage(
      title: "Create New Campaign",
      children: [
        Text('Create New Campaign', style: TextStyle(fontSize: 24)),
        _buildCampaignNameField(),
        _buildCampaignDescriptionField(),
        SelectPlayersButton(
          allowAdding: true,
          onDone: () {
            setState(() {});
          },
          onCharacterSelected: (player) {
            setState(() {
              if (selectedPlayers.contains(player)) {
                selectedPlayers.remove(player);
              } else {
                selectedPlayers.add(player as Player);
              }
            });
          },
          selectedCharacters: selectedPlayers,
        ),
        _buildSelectedPlayersInfo(),
        _buildCreateCampaignButton(),
      ],
    );
  }

  Widget _buildCampaignNameField() {
    return _buildTextField(
      controller: campaignNameController,
      labelText: 'Campaign Name',
      errorText: campaignNameError,
      onChanged: (value) {
        setState(() {
          campaignNameError =
              value.isEmpty ? 'Campaign title is required' : null;
        });
      },
    );
  }

  Widget _buildCampaignDescriptionField() {
    return _buildTextField(
      controller: campaignDescriptionController,
      labelText: 'Enter Campaign Description',
      maxLines: 4,
      minLines: 4,
      keyboardType: TextInputType.multiline,
    );
  }

  Widget _buildTextField({
    required TextEditingController controller,
    required String labelText,
    String? errorText,
    int? maxLines,
    int? minLines,
    TextInputType? keyboardType,
    void Function(String)? onChanged,
  }) {
    return TextField(
      controller: controller,
      decoration: InputDecoration(
        labelText: labelText,
        errorText: errorText,
        border: OutlineInputBorder(),
      ),
      maxLines: maxLines,
      minLines: minLines,
      keyboardType: keyboardType,
      onChanged: onChanged,
    );
  }

  Widget _buildSelectedPlayersInfo() {
    return Text(
      selectedPlayers.isNotEmpty
          ? 'Selected Players: ${selectedPlayers.map((player) => player.name).join(", ")}'
          : 'No players selected',
      style: const TextStyle(color: Colors.grey),
    );
  }

  Widget _buildCreateCampaignButton() {
    return ElevatedButton.icon(
      icon: const Icon(Icons.add),
      onPressed: onSubmitted,
      label: const Padding(
        padding: EdgeInsets.all(16.0),
        child: Text('Create New Campaign', style: TextStyle(fontSize: 16.0)),
      ),
    );
  }

  Future<void> onSubmitted() async {
    setState(() {
      if (campaignNameController.text.isEmpty) {
        campaignNameError = 'Campaign title is required';
      } else {
        campaignNameError = null;
      }
    });
    if (campaignNameError != null) return;

    final newCampaign = Campaign(
      id: "",
      name: campaignNameController.text,
      description: campaignDescriptionController.text,
    );

    final generatedId = (await widget._campaignService.add(newCampaign)).id;
    for (final player in selectedPlayers) {
      await widget._campaignPlayerService.add(
        CampaignPlayer(id: "", campaignId: generatedId, playerId: player.id),
      );
    }
    if (!mounted) return;
    Navigator.pop(context);
  }
}
