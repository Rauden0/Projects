import 'package:tes/models/enemy_battle.dart';

import 'base_service.dart';

class EnemyBattleService extends BaseService<EnemyBattle> {
  EnemyBattleService()
      : super(
          collectionPath: 'enemy_battles',
          fromJson: (json) => EnemyBattle.fromJson(json),
        );

  Stream<List<EnemyBattle>> streamByBattleId(String battleId) {
    return _streamByBattleIdFiltered(battleId);
  }

  Stream<List<EnemyBattle>> _streamByBattleIdFiltered(String battleId) {
    var query = collection.where('battleId', isEqualTo: battleId);
    return query
        .snapshots()
        .map((snapshot) => snapshot.docs.map((doc) => doc.data()).toList());
  }
}
