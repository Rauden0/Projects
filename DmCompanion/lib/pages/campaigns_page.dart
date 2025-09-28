import 'package:flutter/material.dart';
import 'package:tes/models/campaign.dart';
import 'package:tes/models/player.dart';
import 'package:tes/pages/create_new_game_page.dart';
import 'package:tes/pages/player_detail_page.dart';
import 'package:tes/services/campaign_service.dart';
import 'package:tes/services/ioc_container.dart';
import 'package:tes/services/player_service.dart';
import 'package:tes/widgets/campaign/campaign_tile.dart';
import 'package:tes/widgets/handling_stream_builder.dart';
import 'package:tes/widgets/page_template.dart';
import 'package:tes/widgets/rounded_window.dart';

class CampaignsPage extends StatefulWidget {
  final _campaignService = get<CampaignService>();
  final _playerService = get<PlayerService>();

  CampaignsPage({super.key});

  @override
  State<CampaignsPage> createState() => _CampaignsPageState();
}

class _CampaignsPageState extends State<CampaignsPage> {
  bool _showArchived = false;

  @override
  Widget build(BuildContext context) {
    return PageTemplate(
        title: 'DM Companion',
        child: Padding(
          padding: const EdgeInsets.all(16.0),
          child: HandlingStreamBuilder(
            stream: _showArchived
                ? widget._campaignService.stream
                : widget._campaignService.streamByArchived(false),
            builder: (context, campaigns) {
              return _buildCampaignColumn(campaigns, context);
            },
          ),
        ));
  }

  Center _buildCampaignColumn(List<Campaign>? campaigns, BuildContext context) {
    return Center(
      child: Column(
        children: [
          Expanded(
            child: Column(
              mainAxisAlignment: MainAxisAlignment.center,
              crossAxisAlignment: CrossAxisAlignment.end,
              children: [
                ...campaigns!.isEmpty
                    ? []
                    : [
                        _buildCampaignList(campaigns),
                        SizedBox(height: 16.0),
                      ],
                _buildAddButton(context),
                const SizedBox(height: 8.0),
                _buildPlayerStatsButton(context),
              ],
            ),
          ),
          Row(
            crossAxisAlignment: CrossAxisAlignment.center,
            mainAxisAlignment: MainAxisAlignment.end,
            children: [
              Text(
                'Show archived',
                style: TextStyle(fontSize: 16.0),
              ),
              SizedBox(width: 8.0),
              Switch(
                value: _showArchived,
                onChanged: (value) {
                  setState(() {
                    _showArchived = value;
                  });
                },
              ),
            ],
          ),
        ],
      ),
    );
  }

  Widget _buildAddButton(BuildContext context) {
    return ElevatedButton.icon(
      icon: Icon(Icons.add),
      onPressed: () {
        Navigator.of(context).push(
          MaterialPageRoute(
            builder: (context) => CreateNewCampaignPage(),
          ),
        );
      },
      label: Padding(
        padding: const EdgeInsets.all(16.0),
        child: Text(
          'New Campaign',
          style: TextStyle(fontSize: 16.0),
        ),
      ),
    );
  }

  Widget _buildPlayerStatsButton(BuildContext context) {
    return ElevatedButton.icon(
      icon: Icon(Icons.person),
      onPressed: () async {
        final selectedPlayer = await _showPlayerSelectionDialog(context);
        if (selectedPlayer != null) {
          Navigator.push(
            context,
            MaterialPageRoute(
              builder: (context) => PlayerDetailPage(player: selectedPlayer),
            ),
          );
        }
      },
      label: Padding(
        padding: const EdgeInsets.all(16.0),
        child: Text(
          'View Player Stats',
          style: TextStyle(fontSize: 16.0),
        ),
      ),
    );
  }

  Future<Player?> _showPlayerSelectionDialog(BuildContext context) async {
    return showDialog<Player>(
      context: context,
      builder: (context) {
        return HandlingStreamBuilder(
            stream: widget._playerService.stream,
            builder: (context, players) {
              return SimpleDialog(
                title: const Text('Select a Player'),
                children: players.isEmpty
                    ? [
                        Padding(
                          padding: const EdgeInsets.symmetric(horizontal: 24.0),
                          child: const Text("No players"),
                        )
                      ]
                    : players.map<Widget>((player) {
                        return SimpleDialogOption(
                          onPressed: () {
                            Navigator.pop(context, player);
                          },
                          child: Text(player.name),
                        );
                      }).toList(),
              );
            });
      },
    );
  }

  Widget _buildCampaignList(List<Campaign> campaigns) {
    return Flexible(
      child: RoundedWindow(
        child: Padding(
          padding: const EdgeInsets.all(16.0),
          child: SingleChildScrollView(
            scrollDirection: Axis.vertical,
            child: IntrinsicWidth(
              child: Column(
                  crossAxisAlignment: CrossAxisAlignment.stretch,
                  children: _buildCampaigns(campaigns)),
            ),
          ),
        ),
      ),
    );
  }

  List<Widget> _buildCampaigns(List<Campaign> campaigns) {
    List<Widget> campaignList = [];
    for (int i = 0; i < campaigns.length; i++) {
      final campaign = campaigns[i];
      campaignList.add(CampaignTile(
        campaign: campaign,
        onChangeArchived: () =>
            widget._campaignService.setArchived(campaign, !campaign.archived),
        onDelete: () => widget._campaignService.delete(campaign),
      ));
      if (i < campaigns.length - 1) {
        campaignList.add(Divider());
      }
    }
    return campaignList;
  }
}
