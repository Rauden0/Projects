use std::collections::{HashMap, HashSet};
use actix::{AsyncContext, Context, WrapFuture};
use log::info;
use uuid::Uuid;
use shared::{GamePhase, ServerMessage};
use crate::game::entities::turn_manager::TurnManager;
use crate::lobby::{GameInstance, Lobby};

impl Lobby { 
    pub fn handle_leave_game(&mut self, pid: Uuid, game_id: String, ctx: &mut Context<Lobby>) 
    {
    let can_leave_result = if let Some(game) = self.games.get(&game_id) {
        if !game.player_ids.contains(&pid) {
            Err("You are not in this game")
        } else if !matches!(game.phase, GamePhase::WaitingForPlayers) {
            Err("Cannot leave a game in progress")
        } else {
            Ok(())
        }
    } else {
        Err("Game not found")
    };

    if let Err(e) = can_leave_result {
        return self.send_error(pid, e);
    }

    let mut should_remove_game = false;
    {
        if let Some(game) = self.games.get_mut(&game_id) {
            game.player_ids.retain(|&pid_in_game| pid_in_game != pid);
            game.player_id_to_slot.remove(&pid);
            let _ = game.turn_manager.remove_player(pid);

            if game.player_ids.is_empty() {
                should_remove_game = true;
            }
        }
    }

        self.player_to_game.remove(&pid);

        self.send_server_msg(
        pid,
        ServerMessage::Left {
            player_id: pid,
            game_id: game_id.clone(),
        },
    );

    if should_remove_game {
        let gid_for_repo = game_id.clone();
        let repo = self.repo.clone();
        self.games.remove(&game_id);

        ctx.spawn(async move {
            let _ = repo.delete_game_instance(&gid_for_repo).await;
        }.into_actor(self));

        info!("Game {} removed as it has no players left", game_id);
    }

        self.broadcast_lobby_status();
    }
    pub fn handle_join_game(&mut self, pid: Uuid, game_id: String, ctx: &mut Context<Lobby>) {
        let (is_full, already_in) = {
            let Some(game) = self.games.get_mut(&game_id) else {
                return self.send_error(pid, "Game not found");
            };

            if game.player_ids.len() >= game.max_players && !game.player_ids.contains(&pid) {
                return self.send_error(pid, "Game is full");
            }

            let already_in = game.player_ids.contains(&pid);
            if !already_in {
                let slot = game.player_ids.len();
                game.player_id_to_slot.insert(pid, slot);
                game.player_ids.push(pid);
                game.turn_manager.add_player_with_colour(pid);
            }

            (game.player_ids.len() >= game.max_players, already_in)
        };

        if !already_in {
            self.player_to_game.insert(pid, game_id.clone());
        }

        if is_full {
            if let Some(game) = self.games.get_mut(&game_id) {
                if game.phase == GamePhase::WaitingForPlayers {
                    game.phase = GamePhase::InitialPlacementRound1;
                    info!("Game {} starting!", game_id);

                    let first_player = game.player_ids[0];
                    self.send_server_msg(pid, ServerMessage::NextTurn { player_id: first_player });
                }
            }
        }

        self.save_game_async(&game_id, ctx);
        self.send_server_msg(pid, ServerMessage::Joined { player_id: pid, game_id: game_id.clone() });
        self.refresh_game_for_all(&game_id);
        self.broadcast_lobby_status();
    }
    pub fn handle_create_game(&mut self, pid: Uuid, player_count: usize, ctx: &mut Context<Lobby>) {
        let gid = Uuid::new_v4().to_string()[..6].to_string();
        info!("Creating game {} for player {}", gid, pid);

        let mut player_id_to_slot = HashMap::new();
        player_id_to_slot.insert(pid, 0);

        let mut game = GameInstance {
            id: gid.clone(),
            player_ids: vec![pid],
            player_id_to_slot,
            turn_manager: TurnManager::new(player_count, pid),
            max_players: player_count,
            phase: GamePhase::WaitingForPlayers,
            initial_settlements_placed: 0,
            pending_discards: HashSet::new(),
            seven_roller: None,
            knight_mover: None,
            pending_trades: HashMap::new(),
            next_trade_id: 1,
            free_roads_remaining: 0,
            year_of_plenty_pending: None,
            monopoly_pending: None,
        };

        game.turn_manager.add_player_with_colour(pid);

        self.games.insert(gid.clone(), game);
        self.player_to_game.insert(pid, gid.clone());

        self.save_game_async(&gid, ctx);
        self.send_server_msg(pid, ServerMessage::Joined { player_id: pid, game_id: gid.clone() });
        self.refresh_game_for_all(&gid);
        self.broadcast_lobby_status();
    }
}