import 'package:tes/models/battle.dart';
import 'package:tes/services/base_service.dart';

const _collectionPath = 'battles';

class BattleService extends BaseService<Battle> {
  BattleService()
      : super(
          collectionPath: _collectionPath,
          fromJson: (json) => Battle.fromJson(json),
        );

  Future<Battle?> currentBattle(String campaignId) async {
    return await collection
        .where('campaignId', isEqualTo: campaignId)
        .where('isFinished', isEqualTo: false)
        .get()
        .then((snapshot) => snapshot.docs.map((doc) => doc.data()).firstOrNull);
  }
  Stream<List<Battle>> streamByCampaignId(String campaignId) {
    return collection
        .where('campaignId', isEqualTo: campaignId)
        .snapshots()
        .map((snapshot) => snapshot.docs.map((doc) => doc.data()).toList());
  }
}
