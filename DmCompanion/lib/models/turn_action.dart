import 'package:json_annotation/json_annotation.dart';
import 'package:tes/models/base_model.dart';

part 'turn_action.g.dart';

@JsonSerializable()
class TurnAction extends BaseModel {
  final String turnId;
  final String actionId;

  TurnAction({
    required super.id,
    required this.turnId,
    required this.actionId,
  });

  static TurnAction fromJson(Map<String, dynamic> json) =>
      _$TurnActionFromJson(json);

  @override
  Map<String, dynamic> toJson() => _$TurnActionToJson(this);
}
