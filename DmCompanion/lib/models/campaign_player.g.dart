// GENERATED CODE - DO NOT MODIFY BY HAND

part of 'campaign_player.dart';

// **************************************************************************
// JsonSerializableGenerator
// **************************************************************************

CampaignPlayer _$CampaignPlayerFromJson(Map<String, dynamic> json) =>
    CampaignPlayer(
      id: json['id'] as String,
      campaignId: json['campaignId'] as String,
      playerId: json['playerId'] as String,
      isEnemy: json['isEnemy'] as bool? ?? false,
    );

Map<String, dynamic> _$CampaignPlayerToJson(CampaignPlayer instance) =>
    <String, dynamic>{
      'id': instance.id,
      'campaignId': instance.campaignId,
      'playerId': instance.playerId,
      'isEnemy': instance.isEnemy,
    };
