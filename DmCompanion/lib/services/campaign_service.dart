import 'package:tes/models/campaign.dart';
import 'package:tes/services/base_service.dart';
import 'package:tes/services/campaign_player_service.dart';

const _collectionPath = 'campaigns';

class CampaignService extends BaseService<Campaign> {
  final CampaignPlayerService _campaignPlayerService;

  CampaignService(this._campaignPlayerService)
      : super(
            collectionPath: _collectionPath,
            fromJson: (json) => Campaign.fromJson(json));

  Stream<List<Campaign>> streamByArchived(bool archived) => collection
      .where('archived', isEqualTo: archived)
      .snapshots()
      .map((snapshot) => snapshot.docs.map((doc) => doc.data()).toList());

  Future<void> setArchived(Campaign campaign, bool archived) async {
    final updatedCampaign = campaign.copyWith(archived: archived);
    await update(updatedCampaign);
  }

  Future<void> moveTimeForward(
      {required Campaign campaign, int? days, int? months, int? years}) async {
    final unprocessedDay = campaign.day + (days ?? 0);
    final newDay = (unprocessedDay % 30 == 0) ? 30 : unprocessedDay % 30;
    final dayOverflow = (unprocessedDay - 1) ~/ 30;

    final unprocessedMonth = campaign.month + (months ?? 0) + dayOverflow;
    final newMonth = (unprocessedMonth % 12 == 0) ? 12 : unprocessedMonth % 12;
    final monthOverflow = (unprocessedMonth - 1) ~/ 12;

    final newYear = campaign.year + (years ?? 0) + monthOverflow;

    final updatedCampaign = campaign.copyWith(
      year: newYear,
      month: newMonth,
      day: newDay,
    );
    await update(updatedCampaign);
  }

  @override
  Future<void> delete(Campaign entity) async {
    await _campaignPlayerService.deleteByCampaignId(entity.id);
    await super.delete(entity);
  }
}
