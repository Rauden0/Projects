// GENERATED CODE - DO NOT MODIFY BY HAND

part of 'player.dart';

// **************************************************************************
// JsonSerializableGenerator
// **************************************************************************

Player _$PlayerFromJson(Map<String, dynamic> json) => Player(
      id: json['id'] as String,
      name: json['name'] as String,
      health: (json['health'] as num?)?.toInt(),
      maxHealth: (json['maxHealth'] as num).toInt(),
      imagePath: json['imagePath'] as String,
      totalDamageDealt: (json['totalDamageDealt'] as num?)?.toInt() ?? 0,
      totalDamageTaken: (json['totalDamageTaken'] as num?)?.toInt() ?? 0,
      totalHealingDone: (json['totalHealingDone'] as num?)?.toInt() ?? 0,
      totalHealingTaken: (json['totalHealingTaken'] as num?)?.toInt() ?? 0,
    );

Map<String, dynamic> _$PlayerToJson(Player instance) => <String, dynamic>{
      'id': instance.id,
      'name': instance.name,
      'health': instance.health,
      'maxHealth': instance.maxHealth,
      'imagePath': instance.imagePath,
      'totalDamageDealt': instance.totalDamageDealt,
      'totalDamageTaken': instance.totalDamageTaken,
      'totalHealingDone': instance.totalHealingDone,
      'totalHealingTaken': instance.totalHealingTaken,
    };
