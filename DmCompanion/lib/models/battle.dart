import 'package:json_annotation/json_annotation.dart';
import 'package:tes/models/event.dart';

part 'battle.g.dart';

@JsonSerializable()
class Battle extends Event {
  final bool isFinished;

  Battle({
    required super.id,
    required super.campaignId,
    required super.name,
    super.description,
    super.location,
    required super.year,
    required super.month,
    required super.day,
    super.wasSeen,
    this.isFinished = false,
  });

  static Battle fromJson(Map<String, dynamic> json) => _$BattleFromJson(json);

  @override
  Map<String, dynamic> toJson() => _$BattleToJson(this);

  @override
  Battle copyWith({
    String? name,
    String? description,
    String? location,
    int? year,
    int? month,
    int? day,
    bool? wasSeen,
    bool? isFinished,
  }) {
    return Battle(
      id: id,
      campaignId: campaignId,
      name: name ?? this.name,
      description: description ?? this.description,
      location: location ?? this.location,
      year: year ?? this.year,
      month: month ?? this.month,
      day: day ?? this.day,
      wasSeen: wasSeen ?? this.wasSeen,
      isFinished: isFinished ?? this.isFinished,
    );
  }
}
