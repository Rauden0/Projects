import 'package:rxdart/rxdart.dart';
import 'package:tes/models/action.dart';
import 'package:tes/models/character.dart';
import 'package:tes/models/enemy.dart';
import 'package:tes/models/player.dart';
import 'package:tes/services/base_service.dart';
import 'package:tes/services/battle_player_service.dart';
import 'package:tes/services/enemy_battle_service.dart';
import 'package:tes/services/enemy_service.dart';
import 'package:tes/services/player_service.dart';
import 'package:tes/services/turn_service.dart';

class CharacterService extends BaseService<Character> {
  final PlayerService _playerService;
  final EnemyService _enemyService;
  final TurnService _turnService;
  final BattlePlayerService _battlePlayerService;
  final EnemyBattleService _battleEnemyService;

  CharacterService(this._playerService, this._enemyService, this._turnService,
      this._battlePlayerService, this._battleEnemyService)
      : super(
            collectionPath: 'characters',
            fromJson: (json) => Character.fromJson(json));

  Stream<List<Character>> streamByBattleId(String battleId) {
    final enemyStream = _enemyService.stream;
    final playerStream = _playerService.stream;
    final battlePlayerService = _battlePlayerService.streamByBattleId(battleId);
    final battleEnemyService = _battleEnemyService.streamByBattleId(battleId);

    return Rx.combineLatest2(
      battlePlayerService.switchMap((battlePlayers) {
        final playerMap = {
          for (var battlePlayer in battlePlayers)
            battlePlayer.playerId: battlePlayer.order
        };
        return playerStream.map((players) {
          final playersWithOrder = players
              .where((player) => playerMap.containsKey(player.id))
              .map((player) => {
                    'character': player,
                    'order': playerMap[player.id]!,
                  })
              .toList();
          return playersWithOrder;
        });
      }),
      battleEnemyService.switchMap((battleEnemies) {
        final enemyMap = {
          for (var battleEnemy in battleEnemies)
            battleEnemy.enemyId: battleEnemy.order
        };
        return enemyStream.map((enemies) {
          final enemiesWithOrder = enemies
              .where((enemy) => enemyMap.containsKey(enemy.id))
              .map((enemy) => {
                    'character': enemy,
                    'order': enemyMap[enemy.id]!,
                  })
              .toList();
          return enemiesWithOrder;
        });
      }),
      (List<Map<String, dynamic>> playersWithOrder,
          List<Map<String, dynamic>> enemiesWithOrder) {
        final combined = [...playersWithOrder, ...enemiesWithOrder];
        combined.sort((a, b) => a['order'].compareTo(b['order']));
        return combined
            .map<Character>((entry) => entry['character'] as Character)
            .toList();
      },
    );
  }

  Future<void> attack({
    required Character attacker,
    required Character target,
    required int damage,
    required String battleId,
    required int turn,
  }) async {
    final action = Action(
      characterId: attacker.id,
      characterName: attacker.name,
      targetName: target.name,
      type: ActionType.attack,
      targetId: target.id,
      value: damage,
      id: '',
    );

    await _turnService.addActionToTurn(battleId, action, turn);

    if (attacker is Player) {
      final updatedAttacker = attacker.copyWith(
        totalDamageDealt: attacker.totalDamageDealt + damage,
      );
      await _playerService.update(updatedAttacker);
    }

    if (target is Player) {
      final updatedTarget = target.copyWith(
        health: target.health - damage,
        totalDamageTaken: target.totalDamageTaken + damage,
      );
      await _playerService.update(updatedTarget);
    } else if (target is Enemy) {
      final updatedTarget = target.copyWith(
        health: target.health - damage,
      );
      await _enemyService.update(updatedTarget);
    }
  }

  Future<void> heal({
    required Character healer,
    required Character target,
    required int health,
    required String battleId,
    required int turn,
  }) async {
    final action = Action(
      characterName: healer.name,
      targetName: target.name,
      characterId: healer.id,
      type: ActionType.heal,
      targetId: target.id,
      value: health,
      id: '',
    );

    await _turnService.addActionToTurn(battleId, action, turn);

    if (healer is Player) {
      final updatedHealer = healer.copyWith(
        totalHealingDone: healer.totalHealingDone + health,
      );
      await _playerService.update(updatedHealer);
    }

    if (target is Player) {
      final updatedTarget = target.copyWith(
        health: target.health + health,
        totalHealingTaken: target.totalHealingTaken + health,
      );
      await _playerService.update(updatedTarget);
    } else if (target is Enemy) {
      final updatedTarget = target.copyWith(
        health: target.health + health,
      );
      await _enemyService.update(updatedTarget);
    }
  }
}
