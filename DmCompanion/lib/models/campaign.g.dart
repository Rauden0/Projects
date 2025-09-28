// GENERATED CODE - DO NOT MODIFY BY HAND

part of 'campaign.dart';

// **************************************************************************
// JsonSerializableGenerator
// **************************************************************************

Campaign _$CampaignFromJson(Map<String, dynamic> json) => Campaign(
      id: json['id'] as String,
      name: json['name'] as String,
      year: (json['year'] as num?)?.toInt() ?? 1,
      month: (json['month'] as num?)?.toInt() ?? 1,
      day: (json['day'] as num?)?.toInt() ?? 1,
      description: json['description'] as String?,
      archived: json['archived'] as bool? ?? false,
    );

Map<String, dynamic> _$CampaignToJson(Campaign instance) => <String, dynamic>{
      'id': instance.id,
      'name': instance.name,
      'year': instance.year,
      'month': instance.month,
      'day': instance.day,
      'description': instance.description,
      'archived': instance.archived,
    };
