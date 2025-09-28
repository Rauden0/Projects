import 'package:flutter/material.dart';
import 'package:tes/models/turn.dart';
import 'package:tes/widgets/handling_stream_builder.dart';
import 'package:tes/widgets/page_template.dart';

import '../services/ioc_container.dart';
import '../services/turn_service.dart';

class TurnDetailPage extends StatelessWidget {
  final Turn turn;

  TurnDetailPage({super.key, required this.turn});

  @override
  Widget build(BuildContext context) {
    return PageTemplate(
      title: 'Turn Details',
      child: Padding(
        padding: const EdgeInsets.all(16.0),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            Text(
              'Turn ID: ${turn.turn}',
              style: Theme.of(context).textTheme.headlineSmall,
            ),
            const SizedBox(height: 16.0),
            Text(
              'Battle ID: ${turn.battleId}',
              style: Theme.of(context).textTheme.titleSmall,
            ),
            const SizedBox(height: 16.0),
            Text(
              'Actions:',
              style: Theme.of(context).textTheme.headlineSmall,
            ),
            const SizedBox(height: 8.0),
            HandlingStreamBuilder(
              stream: get<TurnService>().streamByTurnId(turn.id),
              builder: (context, actions) {
                return Column(
                  children: actions
                      .map<Widget>(
                        (action) => Column(
                          crossAxisAlignment: CrossAxisAlignment.start,
                          children: [
                            Text(
                              'Action ID: ${action.id}',
                              style: Theme.of(context).textTheme.titleSmall,
                            ),
                            const SizedBox(height: 8.0),
                            Text(
                              'Character Name: ${action.characterName}',
                              style: Theme.of(context).textTheme.titleSmall,
                            ),
                            const SizedBox(height: 8.0),
                            Text(
                              'Target Name: ${action.targetName}',
                              style: Theme.of(context).textTheme.titleSmall,
                            ),
                            const SizedBox(height: 8.0),
                            Text(
                              'Type: ${action.type.name}',
                              style: Theme.of(context).textTheme.titleSmall,
                            ),
                            const SizedBox(height: 8.0),
                            Text(
                              'Amount: ${action.value}',
                              style: Theme.of(context).textTheme.titleSmall,
                            ),
                            const SizedBox(height: 16.0),
                          ],
                        ),
                      )
                      .toList(),
                );
              },
            ),
          ],
        ),
      ),
    );
  }
}
