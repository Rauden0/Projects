import 'package:json_annotation/json_annotation.dart';
import 'package:tes/models/base_model.dart';

part 'campaign_player.g.dart';

@JsonSerializable()
class CampaignPlayer extends BaseModel {
  final String campaignId;
  final String playerId;
  final bool isEnemy;

  CampaignPlayer({
    required super.id,
    required this.campaignId,
    required this.playerId,
    this.isEnemy = false,
  });

  static CampaignPlayer fromJson(Map<String, dynamic> json) =>
      _$CampaignPlayerFromJson(json);

  @override
  Map<String, dynamic> toJson() => _$CampaignPlayerToJson(this);
}
