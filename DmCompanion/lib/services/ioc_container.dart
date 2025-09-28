import 'package:get_it/get_it.dart';
import 'package:tes/services/battle_player_service.dart';
import 'package:tes/services/battle_service.dart';
import 'package:tes/services/campaign_player_service.dart';
import 'package:tes/services/campaign_service.dart';
import 'package:tes/services/character_service.dart';
import 'package:tes/services/event_service.dart';
import 'package:tes/services/player_service.dart';
import 'package:tes/services/turn_action_service.dart';
import 'package:tes/services/turn_service.dart';

import '../auth/auth_service.dart';
import 'action_service.dart';
import 'enemy_battle_service.dart';
import 'enemy_service.dart';

final get = GetIt.instance;

class IoCContainer {
  IoCContainer._();

  static void setup() {
    final campaignPlayerService = CampaignPlayerService();
    final battlePlayerService = BattlePlayerService();
    final battleService = BattleService();
    final enemyBattleService = EnemyBattleService();
    final enemyService = EnemyService(enemyBattleService);
    final turnActionService = TurnActionService();
    final actionService = ActionService();
    final turnService = TurnService(turnActionService, actionService);
    final playerService =
        PlayerService(campaignPlayerService, battlePlayerService);
    final authService = AuthService();
    get.registerSingleton(turnActionService);
    get.registerSingleton(actionService);
    get.registerSingleton(turnService);
    get.registerSingleton(authService);
    get.registerSingleton(CharacterService(
      playerService,
      enemyService,
      turnService,
      battlePlayerService,
      enemyBattleService,
    ));
    get.registerSingleton(playerService);
    get.registerSingleton(enemyBattleService);
    get.registerSingleton(campaignPlayerService);
    get.registerSingleton(CampaignService(campaignPlayerService));
    get.registerSingleton(battleService);
    get.registerSingleton(EventService(battleService));
    get.registerSingleton(battlePlayerService);
    get.registerSingleton(enemyService);
  }
}
