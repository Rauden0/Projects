import 'package:tes/models/turn_action.dart';
import 'package:tes/services/base_service.dart';

class TurnActionService extends BaseService<TurnAction>
{
  static const _collectionPath = 'turnActions';

  TurnActionService()
      : super(
            collectionPath: _collectionPath,
            fromJson: (json) => TurnAction.fromJson(json));

  Future<void> addTurnAction(TurnAction turnAction) async {
    await add(turnAction);
  }

  Stream<List<TurnAction>> streamByTurnId(String turnId) {
    return collection.where('turnId', isEqualTo: turnId).snapshots().map((snapshot) => snapshot.docs.map((doc) => doc.data()).toList());
  }

}