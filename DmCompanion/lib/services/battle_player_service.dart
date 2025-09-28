import 'package:tes/models/battle_player.dart';
import 'package:tes/services/base_service.dart';

const _collectionPath = 'battle_players';

class BattlePlayerService extends BaseService<BattlePlayer> {
  BattlePlayerService()
      : super(
          collectionPath: _collectionPath,
          fromJson: (json) => BattlePlayer.fromJson(json),
        );

  Stream<List<BattlePlayer>> streamByBattleId(String battleId) {
    return collection
        .where('battleId', isEqualTo: battleId)
        .orderBy('order')
        .snapshots()
        .map((snapshot) => snapshot.docs.map((doc) => doc.data()).toList());
  }
}
