import 'package:json_annotation/json_annotation.dart';

import 'action.dart';
import 'base_model.dart';

part 'turn.g.dart';

@JsonSerializable()
class Turn extends BaseModel {
  final String characterId;
  final String battleId;
  final String targetId;
  final int turn;

  Turn({
    required super.id,
    required this.characterId,
    required this.battleId,
    required this.targetId,
    required this.turn,
  });

  static Turn fromJson(Map<String, dynamic> json) => _$TurnFromJson(json);

  @override
  Map<String, dynamic> toJson() => _$TurnToJson(this);

  copyWith({
    String? characterId,
    String? battleId,
    String? targetId,
    int? turn,
    List<Action>? actions,
  }) {
    return Turn(
      id: id,
      characterId: characterId ?? this.characterId,
      battleId: battleId ?? this.battleId,
      targetId: targetId ?? this.targetId,
      turn: turn ?? this.turn,
    );
  }
}