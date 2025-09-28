import 'package:rxdart/rxdart.dart';
import 'package:tes/models/campaign.dart';
import 'package:tes/models/event.dart';
import 'package:tes/services/base_service.dart';
import 'package:tes/services/battle_service.dart';

const _collectionPath = 'events';

class EventService extends BaseService<Event> {
  final BattleService _battleService;

  EventService(this._battleService)
      : super(
            collectionPath: _collectionPath,
            fromJson: (json) => Event.fromJson(json));

  Stream<(List<Event>, int)> streamExtended(Campaign campaign) {
    final eventStream = collection
        .where('campaignId', isEqualTo: campaign.id)
        .orderBy('year')
        .orderBy('month')
        .orderBy('day')
        .snapshots()
        .map((snapshot) => snapshot.docs.map((doc) => doc.data()).toList());
    final battleStream = _battleService.collection
        .where('campaignId', isEqualTo: campaign.id)
        .orderBy('year')
        .orderBy('month')
        .orderBy('day')
        .snapshots()
        .map((snapshot) => snapshot.docs.map((doc) => doc.data()).toList());
    final mergedStream =
        Rx.combineLatest2<List<Event>, List<Event>, List<Event>>(
      eventStream,
      battleStream,
      (events, battles) {
        final combined = [...events, ...battles];
        combined.sort((a, b) {
          final yearComparison = a.year.compareTo(b.year);
          if (yearComparison != 0) return yearComparison;

          final monthComparison = a.month.compareTo(b.month);
          if (monthComparison != 0) return monthComparison;

          return a.day.compareTo(b.day);
        });
        return combined;
      },
    );

    return mergedStream.map((events) {
      final lastPastIndex = events.lastIndexWhere(
        (event) =>
            event.year < campaign.year ||
            (event.year == campaign.year && event.month < campaign.month) ||
            (event.year == campaign.year &&
                event.month == campaign.month &&
                event.day <= campaign.day),
      );

      return (
        events,
        lastPastIndex,
      );
    });
  }
}
