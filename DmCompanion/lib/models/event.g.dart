// GENERATED CODE - DO NOT MODIFY BY HAND

part of 'event.dart';

// **************************************************************************
// JsonSerializableGenerator
// **************************************************************************

Event _$EventFromJson(Map<String, dynamic> json) => Event(
      id: json['id'] as String,
      campaignId: json['campaignId'] as String,
      name: json['name'] as String,
      description: json['description'] as String?,
      location: json['location'] as String?,
      year: (json['year'] as num).toInt(),
      month: (json['month'] as num).toInt(),
      day: (json['day'] as num).toInt(),
      wasSeen: json['wasSeen'] as bool? ?? false,
    );

Map<String, dynamic> _$EventToJson(Event instance) => <String, dynamic>{
      'id': instance.id,
      'campaignId': instance.campaignId,
      'name': instance.name,
      'description': instance.description,
      'location': instance.location,
      'year': instance.year,
      'month': instance.month,
      'day': instance.day,
      'wasSeen': instance.wasSeen,
    };
