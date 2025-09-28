import 'package:tes/services/turn_action_service.dart';
import '../models/action.dart';
import '../models/turn.dart';
import '../models/turn_action.dart';
import 'action_service.dart';
import 'base_service.dart';

class TurnService extends BaseService<Turn> {
  static const _collectionPath = 'turns';
  final TurnActionService _turnActionService;
  final ActionService _actionService;

  TurnService(this._turnActionService, this._actionService)
      : super(
            collectionPath: _collectionPath,
            fromJson: (json) => Turn.fromJson(json));

  Future<void> addActionToTurn(
      String battleId, Action action, int currentTurn) async {
    final Turn? turn = await getTurnById(currentTurn,battleId);
    if (turn == null) {
      final newTurn = Turn(
        id: '',
        characterId: action.characterId,
        battleId: battleId,
        targetId: action.targetId,
        turn: currentTurn,
      );
      final turn = await add(newTurn);
      final actionDoc = await _actionService.add(action);
      await linkTurnAndAction(turn.id, actionDoc.id);
    } else {
      final actionDoc =  await _actionService.add(action);
      await linkTurnAndAction(turn.id, actionDoc.id);
    }
  }


  Future<void> linkTurnAndAction(String turnId, String actionId) async {
    final turnAction = TurnAction(
      id: '',
      turnId: turnId,
      actionId: actionId,
    );
    _turnActionService.add(turnAction);
  }

  Future<Turn?> getTurnById(int turnId,String battleId) async {
    final snapshot = await collection.where('turn', isEqualTo: turnId).where('battleId', isEqualTo: battleId).get();
    if (snapshot.docs.isNotEmpty) {
      return snapshot.docs.first.data();
    }
    return null;
  }

  Stream<List<Turn>> streamByBattleId(String battleId) {
    return collection
        .where('battleId', isEqualTo: battleId)
        .snapshots()
        .map((snapshot) => snapshot.docs.map((doc) => doc.data()).toList());
  }

  Stream<List<Action>> streamByTurnId(String turnId) {
    final Stream<List<TurnAction>> turnActionsStream =
    _turnActionService.streamByTurnId(turnId);

    return turnActionsStream.asyncMap((turnActions) async {
      final List<Future<Action?>> actionFutures = turnActions
          .map((turnAction) => _actionService.streamById(turnAction.actionId).first)
          .toList();
      final List<Action?> actions = await Future.wait(actionFutures);
      return actions.whereType<Action>().toList();
    });
  }

}
