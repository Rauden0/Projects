import 'package:json_annotation/json_annotation.dart';
import 'package:tes/models/character.dart';

part 'enemy.g.dart';

@JsonSerializable()
class Enemy extends Character {
  final int rewardPoints;

  Enemy({
    required super.id,
    required super.name,
    super.health,
    required super.maxHealth,
    required super.imagePath,
    this.rewardPoints = 0,
  });

  static Enemy fromJson(Map<String, dynamic> json) => _$EnemyFromJson(json);

  @override
  Map<String, dynamic> toJson() => _$EnemyToJson(this);

  @override
  Enemy copyWith({
    String? name,
    int? health,
    int? maxHealth,
    String? imagePath,
    int? rewardPoints,
  }) {
    return Enemy(
      id: id,
      name: name ?? this.name,
      health: health ?? this.health,
      maxHealth: maxHealth ?? this.maxHealth,
      imagePath: imagePath ?? this.imagePath,
      rewardPoints: rewardPoints ?? this.rewardPoints,
    );
  }
}
