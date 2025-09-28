import 'package:json_annotation/json_annotation.dart';

import 'base_model.dart';

part 'action.g.dart';

@JsonSerializable()
class Action extends BaseModel {
  final String characterId;
  final ActionType type;
  final String targetId;
  final int value;
  final String characterName;
  final String targetName;
  Action({
    required this.characterId,
    required this.characterName,
    required this.targetName,
    required this.type,
    required this.targetId,
    required this.value,
    required super.id,
  });

  static Action fromJson(Map<String, dynamic> json) => _$ActionFromJson(json);

  @override
  Map<String, dynamic> toJson() => _$ActionToJson(this);
}

enum ActionType { attack, heal }
