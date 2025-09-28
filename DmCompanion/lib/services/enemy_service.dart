import 'package:rxdart/rxdart.dart';
import 'package:tes/models/enemy.dart';

import 'base_service.dart';
import 'enemy_battle_service.dart';

class EnemyService extends BaseService<Enemy> {
  final EnemyBattleService _battleEnemyService;

  EnemyService(this._battleEnemyService)
      : super(
          collectionPath: 'enemies',
          fromJson: (json) => Enemy.fromJson(json),
        );

  Stream<List<Enemy>> streamByBattleId(String battleId) {
    final enemyStream = stream;
    final battleEnemyService = _battleEnemyService.streamByBattleId(battleId);
    return battleEnemyService.switchMap((battleEnemies) {
      final enemyIds =
          battleEnemies.map((battleEnemy) => battleEnemy.enemyId).toList();
      return enemyStream.map((enemies) {
        final enemyMap = {for (var enemy in enemies) enemy.id: enemy};
        return enemyIds.map((id) => enemyMap[id]).whereType<Enemy>().toList();
      });
    });
  }
}
