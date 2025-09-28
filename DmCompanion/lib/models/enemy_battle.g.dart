// GENERATED CODE - DO NOT MODIFY BY HAND

part of 'enemy_battle.dart';

// **************************************************************************
// JsonSerializableGenerator
// **************************************************************************

EnemyBattle _$EnemyBattleFromJson(Map<String, dynamic> json) => EnemyBattle(
      id: json['id'] as String,
      battleId: json['battleId'] as String,
      enemyId: json['enemyId'] as String,
      order: (json['order'] as num).toInt(),
    );

Map<String, dynamic> _$EnemyBattleToJson(EnemyBattle instance) =>
    <String, dynamic>{
      'id': instance.id,
      'battleId': instance.battleId,
      'enemyId': instance.enemyId,
      'order': instance.order,
    };
