// GENERATED CODE - DO NOT MODIFY BY HAND

part of 'turn_action.dart';

// **************************************************************************
// JsonSerializableGenerator
// **************************************************************************

TurnAction _$TurnActionFromJson(Map<String, dynamic> json) => TurnAction(
      id: json['id'] as String,
      turnId: json['turnId'] as String,
      actionId: json['actionId'] as String,
    );

Map<String, dynamic> _$TurnActionToJson(TurnAction instance) =>
    <String, dynamic>{
      'id': instance.id,
      'turnId': instance.turnId,
      'actionId': instance.actionId,
    };
