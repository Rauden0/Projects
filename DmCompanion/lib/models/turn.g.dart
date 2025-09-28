// GENERATED CODE - DO NOT MODIFY BY HAND

part of 'turn.dart';

// **************************************************************************
// JsonSerializableGenerator
// **************************************************************************

Turn _$TurnFromJson(Map<String, dynamic> json) => Turn(
      id: json['id'] as String,
      characterId: json['characterId'] as String,
      battleId: json['battleId'] as String,
      targetId: json['targetId'] as String,
      turn: (json['turn'] as num).toInt(),
    );

Map<String, dynamic> _$TurnToJson(Turn instance) => <String, dynamic>{
      'id': instance.id,
      'characterId': instance.characterId,
      'battleId': instance.battleId,
      'targetId': instance.targetId,
      'turn': instance.turn,
    };
