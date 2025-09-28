// GENERATED CODE - DO NOT MODIFY BY HAND

part of 'battle_player.dart';

// **************************************************************************
// JsonSerializableGenerator
// **************************************************************************

BattlePlayer _$BattlePlayerFromJson(Map<String, dynamic> json) => BattlePlayer(
      id: json['id'] as String,
      battleId: json['battleId'] as String,
      playerId: json['playerId'] as String,
      order: (json['order'] as num).toInt(),
    );

Map<String, dynamic> _$BattlePlayerToJson(BattlePlayer instance) =>
    <String, dynamic>{
      'id': instance.id,
      'battleId': instance.battleId,
      'playerId': instance.playerId,
      'order': instance.order,
    };
