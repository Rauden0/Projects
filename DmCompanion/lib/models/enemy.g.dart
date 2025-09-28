// GENERATED CODE - DO NOT MODIFY BY HAND

part of 'enemy.dart';

// **************************************************************************
// JsonSerializableGenerator
// **************************************************************************

Enemy _$EnemyFromJson(Map<String, dynamic> json) => Enemy(
      id: json['id'] as String,
      name: json['name'] as String,
      health: (json['health'] as num?)?.toInt(),
      maxHealth: (json['maxHealth'] as num).toInt(),
      imagePath: json['imagePath'] as String,
      rewardPoints: (json['rewardPoints'] as num?)?.toInt() ?? 0,
    );

Map<String, dynamic> _$EnemyToJson(Enemy instance) => <String, dynamic>{
      'id': instance.id,
      'name': instance.name,
      'health': instance.health,
      'maxHealth': instance.maxHealth,
      'imagePath': instance.imagePath,
      'rewardPoints': instance.rewardPoints,
    };
