import 'package:json_annotation/json_annotation.dart';
import 'package:tes/models/base_model.dart';

part 'battle_player.g.dart';

@JsonSerializable()
class BattlePlayer extends BaseModel {
  final String battleId;
  final String playerId;
  final int order;

  BattlePlayer({
    required super.id,
    required this.battleId,
    required this.playerId,
    required this.order,
  });

  static BattlePlayer fromJson(Map<String, dynamic> json) =>
      _$BattlePlayerFromJson(json);

  @override
  Map<String, dynamic> toJson() => _$BattlePlayerToJson(this);
}
