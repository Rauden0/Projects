import 'dart:async';
import 'package:flutter/material.dart';
import 'package:tes/models/character.dart';
import 'package:tes/services/enemy_service.dart';
import 'package:tes/services/ioc_container.dart';
import 'package:tes/services/player_service.dart';
import 'package:tes/widgets/handling_stream_builder.dart';
import 'package:tes/widgets/rounded_window.dart';
import 'health_bar.dart';

class CharacterAvatar extends StatelessWidget {
  final _playerService = get<PlayerService>();
  final _enemyService = get<EnemyService>();
  final String playerId;

  CharacterAvatar({super.key, required this.playerId});

  @override
  @override
  Widget build(BuildContext context) {
    return ConstrainedBox(
      constraints: BoxConstraints(
        minWidth: 150,
        minHeight: 200,
        maxWidth: 300,
        maxHeight: 400,
      ),
      child: RoundedWindow(
        color: Theme.of(context).colorScheme.outline,
        child: _buildBody(context),
      ),
    );
  }

  Widget _buildBody(BuildContext context) {
    return Padding(
      padding: const EdgeInsets.all(16.0),
      child: HandlingStreamBuilder(
        stream: _combinedStream(),
        builder: (context, character) {
          if (character == null) {
            return Center(
              child: Text('Character not found'),
            );
          }

          return _buildCharacterDetails(
            context,
            character as Character,
            isEnemy: false,
          );
        },
      ),
    );
  }

  Stream<dynamic> _combinedStream() async* {
    final playerStream = _playerService.streamById(playerId);
    final enemyStream = _enemyService.streamById(playerId);

    final controller = StreamController<dynamic>();

    void handleSubscription(Stream<dynamic> stream) {
      stream.listen((character) {
        if (character != null) {
          if (!controller.isClosed) {
            controller.add(character);
            controller.close();
          }
        }
      }, onDone: () {
        if (!controller.isClosed) {
          controller.add(null);
        }
      });
    }

    handleSubscription(enemyStream);
    handleSubscription(playerStream);

    yield* controller.stream;
  }

  Widget _buildCharacterDetails(BuildContext context, Character character,
      {required bool isEnemy}) {
    return LayoutBuilder(
      builder: (context, constraints) {
        bool showImage = constraints.maxHeight > 250;

        return Column(
          mainAxisAlignment: MainAxisAlignment.spaceBetween,
          children: [
            showImage
                ? Flexible(
                    flex: 3,
                    child: _buildPlayerImage(context, character),
                  )
                : Container(),
            Flexible(
              flex: 1,
              child: Text(
                character.name,
                style: TextStyle(
                  fontWeight: FontWeight.bold,
                  fontSize: 16.0,
                ),
                overflow: TextOverflow.ellipsis,
                maxLines: 1,
              ),
            ),
            Flexible(
              flex: 1,
              child: HealthBar(
                player: character,
              ),
            )
          ],
        );
      },
    );
  }

  Widget _buildPlayerImage(BuildContext context, Character player) {
    return Container(
      width: double.infinity,
      height: double.infinity,
      decoration: BoxDecoration(
        borderRadius: BorderRadius.circular(16.0), // Adjust border radius
        border: Border.all(
          color: Theme.of(context).colorScheme.onSecondary,
          width: 2.0,
        ),
      ),
      child: ClipRRect(
        borderRadius: BorderRadius.circular(16.0),
        child: Image(
          image: AssetImage(player.imagePath),
          fit: BoxFit.cover,
        ),
      ),
    );
  }
}
