import 'package:tes/models/action.dart';
import 'package:tes/services/base_service.dart';

class ActionService extends BaseService<Action> {
  static const _collectionPath = 'actions';

  ActionService()
      : super(
            collectionPath: _collectionPath,
            fromJson: (json) => Action.fromJson(json));

  Future<void> addAction(Action action) async {
    await add(action);
  }
}
