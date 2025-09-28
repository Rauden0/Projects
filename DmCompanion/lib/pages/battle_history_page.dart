import 'package:flutter/material.dart';
import 'package:tes/models/battle.dart';
import 'package:tes/services/battle_service.dart';
import 'package:tes/services/ioc_container.dart';
import 'package:tes/widgets/handling_stream_builder.dart';
import 'package:tes/widgets/page_template.dart';
import 'battle_detail_page.dart';

class BattleHistoryPage extends StatefulWidget {
  final String campaignId;

  BattleHistoryPage({super.key, required this.campaignId});

  @override
  State<BattleHistoryPage> createState() => _BattleHistoryPageState();
}

class _BattleHistoryPageState extends State<BattleHistoryPage> {
  final BattleService _battleService = get<BattleService>();

  @override
  Widget build(BuildContext context) {
    return PageTemplate(
      title: 'Battles',
      child: HandlingStreamBuilder<List<Battle>>(
        stream: _battleService.streamByCampaignId(widget.campaignId),
        builder: (context, battles) {
          if (battles.isEmpty) {
            return Center(
              child: Text('No battles found'),
            );
          }

          return ListView.builder(
            itemCount: battles.length,
            itemBuilder: (context, index) {
              final battle = battles[index];

              return ListTile(
                title: Text(battle.name),
                subtitle: Text('Finished: ${battle.isFinished ? 'Yes' : 'No'}'),
                onTap: () {
                  Navigator.push(
                    context,
                    MaterialPageRoute(
                      builder: (context) => BattleDetailPage(battle: battle),
                    ),
                  );
                },
              );
            },
          );
        },
      ),
    );
  }
}
