import 'package:flutter/material.dart';
import 'package:tes/models/battle.dart';
import 'package:tes/pages/turn_detail_page.dart';
import 'package:tes/services/ioc_container.dart';
import 'package:tes/services/turn_service.dart';
import 'package:tes/widgets/handling_stream_builder.dart';
import 'package:tes/widgets/page_template.dart';

class BattleDetailPage extends StatelessWidget {
  final Battle battle;

  BattleDetailPage({super.key, required this.battle});

  final _turnService = get<TurnService>();

  @override
  Widget build(BuildContext context) {
    return PageTemplate(
      title: battle.name,
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Padding(
            padding: const EdgeInsets.all(16.0),
            child: Text(
              'Description: ${battle.description ?? 'No description'}',
              style: TextStyle(fontSize: 16),
            ),
          ),
          Padding(
            padding: const EdgeInsets.all(16.0),
            child: Text(
              'Location: ${battle.location ?? 'Unknown'}',
              style: TextStyle(fontSize: 16),
            ),
          ),
          Padding(
            padding: const EdgeInsets.all(16.0),
            child: Text(
              'Date: ${battle.day}-${battle.month}-${battle.year}',
              style: TextStyle(fontSize: 16),
            ),
          ),
          Padding(
            padding: const EdgeInsets.all(16.0),
            child: Text(
              'Finished: ${battle.isFinished ? 'Yes' : 'No'}',
              style: TextStyle(fontSize: 16),
            ),
          ),
          HandlingStreamBuilder(
            stream: _turnService.streamByBattleId(battle.id),
            builder: (context, turns) {
              return Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  Padding(
                    padding: const EdgeInsets.all(16.0),
                    child: Text(
                      'Turns:',
                      style: TextStyle(fontSize: 16),
                    ),
                  ),
                  for (final turn in turns)
                    Padding(
                      padding: const EdgeInsets.all(16.0),
                      child: ElevatedButton(
                        onPressed: () {
                          Navigator.of(context).push(
                            MaterialPageRoute(
                              builder: (context) => TurnDetailPage(turn: turn),
                            ),
                          );
                        },
                        child: Text('Turn ${turn.id}'),
                      ),
                    ),
                ],
              );
            },
          ),
        ],
      ),
    );
  }
}
