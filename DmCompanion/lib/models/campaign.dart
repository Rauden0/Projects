import 'package:json_annotation/json_annotation.dart';
import 'package:tes/models/base_model.dart';

part 'campaign.g.dart';

@JsonSerializable()
class Campaign extends BaseModel {
  final String name;
  final int year;
  final int month;
  final int day;
  final String? description;
  final bool archived;

  Campaign({
    required super.id,
    required this.name,
    this.year = 1,
    this.month = 1,
    this.day = 1,
    this.description,
    this.archived = false,
  });

  static Campaign fromJson(Map<String, dynamic> json) =>
      _$CampaignFromJson(json);

  @override
  Map<String, dynamic> toJson() => _$CampaignToJson(this);

  Campaign copyWith({
    String? name,
    int? year,
    int? month,
    int? day,
    String? description,
    bool? archived,
  }) {
    return Campaign(
      id: id,
      name: name ?? this.name,
      year: year ?? this.year,
      month: month ?? this.month,
      day: day ?? this.day,
      description: description ?? this.description,
      archived: archived ?? this.archived,
    );
  }

  @override
  bool operator ==(Object other) {
    if (identical(this, other)) return true;

    return other is Campaign &&
        other.id == id &&
        other.year == year &&
        other.month == month &&
        other.day == day;
  }

  @override
  int get hashCode {
    return id.hashCode ^ year.hashCode ^ month.hashCode ^ day.hashCode;
  }
}
