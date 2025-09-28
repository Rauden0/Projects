import 'package:json_annotation/json_annotation.dart';
import 'package:tes/models/base_model.dart';

part 'character.g.dart';

@JsonSerializable()
class Character extends BaseModel {
  final String name;
  final int health;
  final int maxHealth;
  final String imagePath;

  Character({
    required super.id,
    required this.name,
    int? health,
    required this.maxHealth,
    required this.imagePath,
  }) : health = health ?? maxHealth;

  static Character fromJson(Map<String, dynamic> json) =>
      _$CharacterFromJson(json);

  @override
  Map<String, dynamic> toJson() => _$CharacterToJson(this);

  Character copyWith({
    String? name,
    int? health,
    int? maxHealth,
    String? imagePath,
  }) {
    return Character(
      id: id,
      name: name ?? this.name,
      health: health ?? this.health,
      maxHealth: maxHealth ?? this.maxHealth,
      imagePath: imagePath ?? this.imagePath,
    );
  }

  bool get isAlive => health > 0;
}
