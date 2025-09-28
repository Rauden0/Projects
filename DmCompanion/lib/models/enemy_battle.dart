import 'package:json_annotation/json_annotation.dart';

import 'base_model.dart';

part 'enemy_battle.g.dart';

@JsonSerializable()
class EnemyBattle extends BaseModel {
  final String battleId;
  final String enemyId;
  final int order;

  EnemyBattle({
    required super.id,
    required this.battleId,
    required this.enemyId,
    required this.order,
  });

  static EnemyBattle fromJson(Map<String, dynamic> json) =>
      _$EnemyBattleFromJson(json);

  @override
  Map<String, dynamic> toJson() => _$EnemyBattleToJson(this);
}
