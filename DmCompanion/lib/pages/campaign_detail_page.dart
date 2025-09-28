import 'package:flutter/material.dart';
import 'package:tes/models/battle.dart';
import 'package:tes/models/campaign.dart';
import 'package:tes/models/player.dart';
import 'package:tes/pages/battle_page.dart';
import 'package:tes/services/battle_service.dart';
import 'package:tes/services/campaign_service.dart';
import 'package:tes/services/ioc_container.dart';
import 'package:tes/services/player_service.dart';
import 'package:tes/utils/format_date.dart';
import 'package:tes/widgets/campaign/campaign_date_button.dart';
import 'package:tes/widgets/campaign/campaign_detail_options.dart';
import 'package:tes/widgets/character/character_list.dart';
import 'package:tes/widgets/event/event_list.dart';
import 'package:tes/widgets/handling_future_builder.dart';
import 'package:tes/widgets/handling_stream_builder.dart';
import 'package:tes/widgets/page_template.dart';

class CampaignDetailPage extends StatelessWidget {
  final _campaignService = get<CampaignService>();
  final _playerService = get<PlayerService>();
  final _battleService = get<BattleService>();

  final String campaignId;

  CampaignDetailPage({super.key, required this.campaignId});

  @override
  Widget build(BuildContext context) {
    return HandlingFutureBuilder(
      allowNull: true,
      future: _battleService.currentBattle(campaignId),
      builder: (context, currentBattle) {
        if (currentBattle != null) {
          WidgetsBinding.instance.addPostFrameCallback((_) {
            Navigator.push(context, MaterialPageRoute(builder: (context) {
              return BattlePage(battleId: currentBattle.id);
            }));
          });
        }

        return _buildStreamBuilder(currentBattle);
      },
    );
  }

  Widget _buildStreamBuilder(Battle? currentBattle) {
    return HandlingStreamBuilder(
      stream: _campaignService.streamById(campaignId),
      builder: (context, campaign) {
        if (campaign == null) {
          return PageTemplate(
            title: 'Campaign not found',
            child: Center(
              child: Text('Campaign not found'),
            ),
          );
        }

        return PageTemplate(
          title: campaign.name,
          child: Padding(
            padding: const EdgeInsets.all(16.0),
            child: HandlingStreamBuilder(
              stream: _playerService.streamByCampaignIdFiltered(campaign.id),
              builder: (context, players) {
                return LayoutBuilder(
                  builder: (BuildContext context, BoxConstraints constraints) {
                    if (currentBattle != null) {
                      ElevatedButton(
                        onPressed: () {
                          Navigator.push(context,
                              MaterialPageRoute(builder: (context) {
                            return BattlePage(battleId: currentBattle.id);
                          }));
                        },
                        child: Text('Continue battle'),
                      );
                    }
                    if (constraints.maxWidth > 800) {
                      return Row(
                        crossAxisAlignment: CrossAxisAlignment.stretch,
                        children: [
                          CharacterList(players: players),
                          SizedBox(width: 16.0),
                          Flexible(
                            child: _buildMainColumn(campaign, false, players),
                          ),
                        ],
                      );
                    }
                    return _buildMainColumn(campaign, true, players);
                  },
                );
              },
            ),
          ),
        );
      },
    );
  }

  Widget _buildMainColumn(
      Campaign campaign, bool showPlayerListButton, List<Player> players) {
    return Column(
      mainAxisAlignment: MainAxisAlignment.spaceBetween,
      children: [
        _buildDate(campaign),
        SizedBox(height: 32.0),
        ...(showPlayerListButton
            ? [
                Flexible(
                  child: _buildEventList(campaign),
                ),
                SizedBox(height: 32.0),
                CampaignDetailOptions(
                  showPlayerListButton: showPlayerListButton,
                  players: players,
                  campaign: campaign,
                ),
              ]
            : [
                CampaignDetailOptions(
                  showPlayerListButton: showPlayerListButton,
                  players: players,
                  campaign: campaign,
                ),
                SizedBox(height: 32.0),
                Flexible(
                  child: _buildEventList(campaign),
                ),
              ])
      ],
    );
  }

  Widget _buildEventList(Campaign campaign) {
    return LayoutBuilder(
      builder: (context, constraints) {
        if (constraints.maxWidth > 800) {
          return SizedBox(
            height: 200.0,
            child: EventList(
              campaign: campaign,
              scrollDirection: Axis.horizontal,
            ),
          );
        }

        return EventList(
          campaign: campaign,
          scrollDirection: Axis.vertical,
        );
      },
    );
  }

  Widget _buildDate(Campaign campaign) {
    return Column(
      children: [
        Text(
          formatDate(campaign.year, campaign.month, campaign.day),
          style: TextStyle(fontSize: 32.0),
        ),
        SizedBox(height: 8.0),
        CampaignDateButton(
          campaign: campaign,
        ),
      ],
    );
  }
}
