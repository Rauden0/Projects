import 'package:json_annotation/json_annotation.dart';
import 'package:tes/models/base_model.dart';

part 'event.g.dart';

@JsonSerializable()
class Event extends BaseModel {
  final String campaignId;
  final String name;
  final String? description;
  final String? location;
  final int year;
  final int month;
  final int day;
  final bool wasSeen;

  Event({
    required super.id,
    required this.campaignId,
    required this.name,
    this.description,
    this.location,
    required this.year,
    required this.month,
    required this.day,
    this.wasSeen = false,
  });

  static Event fromJson(Map<String, dynamic> json) => _$EventFromJson(json);

  @override
  Map<String, dynamic> toJson() => _$EventToJson(this);

  Event copyWith({
    String? name,
    String? description,
    String? location,
    int? year,
    int? month,
    int? day,
    bool? wasSeen,
  }) {
    return Event(
      id: id,
      campaignId: campaignId,
      name: name ?? this.name,
      description: description ?? this.description,
      location: location ?? this.location,
      year: year ?? this.year,
      month: month ?? this.month,
      day: day ?? this.day,
      wasSeen: wasSeen ?? this.wasSeen,
    );
  }
}
