import 'package:json_annotation/json_annotation.dart';

import 'character.dart';

part 'player.g.dart';

@JsonSerializable()
class Player extends Character {
  final int totalDamageDealt;
  final int totalDamageTaken;
  final int totalHealingDone;
  final int totalHealingTaken;

  Player({
    required super.id,
    required super.name,
    super.health,
    required super.maxHealth,
    required super.imagePath,
    this.totalDamageDealt = 0,
    this.totalDamageTaken = 0,
    this.totalHealingDone = 0,
    this.totalHealingTaken = 0,
  });

  static Player fromJson(Map<String, dynamic> json) => _$PlayerFromJson(json);

  @override
  Map<String, dynamic> toJson() => _$PlayerToJson(this);

  @override
  Player copyWith({
    String? name,
    int? health,
    int? maxHealth,
    String? imagePath,
    int? totalDamageDealt,
    int? totalDamageTaken,
    int? totalHealingDone,
    int? totalHealingTaken,
  }) {
    return Player(
      id: id,
      name: name ?? this.name,
      health: health ?? this.health,
      maxHealth: maxHealth ?? this.maxHealth,
      imagePath: imagePath ?? this.imagePath,
      totalDamageDealt: totalDamageDealt ?? this.totalDamageDealt,
      totalDamageTaken: totalDamageTaken ?? this.totalDamageTaken,
      totalHealingDone: totalHealingDone ?? this.totalHealingDone,
      totalHealingTaken: totalHealingTaken ?? this.totalHealingTaken,
    );
  }
}
