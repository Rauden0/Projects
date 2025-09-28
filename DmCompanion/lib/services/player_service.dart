import 'package:rxdart/rxdart.dart';
import 'package:tes/models/character.dart';
import 'package:tes/models/player.dart';
import 'package:tes/services/base_service.dart';
import 'package:tes/services/battle_player_service.dart';
import 'package:tes/services/campaign_player_service.dart';

const _collectionPath = 'players';

class PlayerService extends BaseService<Player> {
  final CampaignPlayerService _campaignPlayerService;
  final BattlePlayerService _battlePlayerService;

  PlayerService(this._campaignPlayerService, this._battlePlayerService)
      : super(
            collectionPath: _collectionPath,
            fromJson: (json) => Player.fromJson(json));

  Stream<List<Player>> streamByCampaignId(String campaignId) {
    return streamByCampaignIdFiltered(campaignId);
  }

  Stream<List<Player>> streamByCampaignIdFiltered(String campaignId) {
    final playerStream = stream;
    final campaignPlayerStream =
        _campaignPlayerService.streamByCampaignIdFiltered(campaignId);

    return campaignPlayerStream.switchMap((campaignPlayers) {
      final playerIds = campaignPlayers
          .map((campaignPlayer) => campaignPlayer.playerId)
          .toSet();
      return playerStream.map((players) =>
          players.where((player) => playerIds.contains(player.id)).toList());
    });
  }

  Stream<List<Player>> streamByBattleId(String battleId) {
    final playerStream = stream;
    final battlePlayerStream = _battlePlayerService.streamByBattleId(battleId);

    return battlePlayerStream.switchMap((battlePlayers) {
      final playerIds =
          battlePlayers.map((battlePlayer) => battlePlayer.playerId).toList();
      return playerStream.map((players) {
        final playerMap = {for (var player in players) player.id: player};
        return playerIds
            .map((id) => playerMap[id])
            .whereType<Player>()
            .toList();
      });
    });
  }

  Future<void> changeHealth({
    required Character player,
    required int delta,
  }) async {
    final updatedPlayer = player.copyWith(
      health: player.health + delta,
    );
    await update(updatedPlayer as Player);
  }
}
