import 'package:tes/models/campaign_player.dart';
import 'package:tes/services/base_service.dart';

const _collectionPath = 'campaign_players';

class CampaignPlayerService extends BaseService<CampaignPlayer> {
  CampaignPlayerService()
      : super(
          collectionPath: _collectionPath,
          fromJson: (json) => CampaignPlayer.fromJson(json),
        );

  Stream<List<CampaignPlayer>> streamByCampaignId(String campaignId) {
    return streamByCampaignIdFiltered(campaignId);
  }

  Stream<List<CampaignPlayer>> streamByCampaignIdFiltered(String campaignId) {
    var query = collection.where('campaignId', isEqualTo: campaignId);
    return query
        .snapshots()
        .map((snapshot) => snapshot.docs.map((doc) => doc.data()).toList());
  }

  Future<void> deleteByCampaignId(String campaignId) async {
    await collection
        .where('campaignId', isEqualTo: campaignId)
        .get()
        .then((snapshot) {
      for (var doc in snapshot.docs) {
        doc.reference.delete();
      }
    });
  }
}
