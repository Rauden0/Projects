// GENERATED CODE - DO NOT MODIFY BY HAND

part of 'action.dart';

// **************************************************************************
// JsonSerializableGenerator
// **************************************************************************

Action _$ActionFromJson(Map<String, dynamic> json) => Action(
      characterId: json['characterId'] as String,
      characterName: json['characterName'] as String,
      targetName: json['targetName'] as String,
      type: $enumDecode(_$ActionTypeEnumMap, json['type']),
      targetId: json['targetId'] as String,
      value: (json['value'] as num).toInt(),
      id: json['id'] as String,
    );

Map<String, dynamic> _$ActionToJson(Action instance) => <String, dynamic>{
      'id': instance.id,
      'characterId': instance.characterId,
      'type': _$ActionTypeEnumMap[instance.type]!,
      'targetId': instance.targetId,
      'value': instance.value,
      'characterName': instance.characterName,
      'targetName': instance.targetName,
    };

const _$ActionTypeEnumMap = {
  ActionType.attack: 'attack',
  ActionType.heal: 'heal',
};
