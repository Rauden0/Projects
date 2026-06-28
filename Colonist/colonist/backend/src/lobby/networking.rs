use actix::{AsyncContext, Context, WrapFuture};
use log::{error, info, warn};
use uuid::Uuid;
use shared::ServerMessage::MustMoveRobber;
use crate::lobby::Lobby;
use crate::network::message::ServerMessage;
use crate::game::entities::bonus_points::BonusCard;
use crate::game::entities::building::VertexBuilding;
use crate::game::entities::game_instance::GameInstance;

impl Lobby
{
    pub(crate) fn send_server_msg(&self, pid: Uuid, msg: shared::ServerMessage) {
        match serde_json::to_string(&msg) {
            Ok(json_string) => {
                let actix_msg = ServerMessage(json_string);
                if let Some(addr) = self.sessions.get(&pid) {
                    let _ = addr.do_send(actix_msg);
                } else {
                    warn!("Failed to send: Player {} session not found", pid);
                }
            },
            Err(e) => error!("Serialization error: {}", e),
        }
    }

    pub fn broadcast_to_game(&self, game_id: &str, msg: shared::ServerMessage) {
        if let Some(game) = self.games.get(game_id) {
            if let Ok(json_string) = serde_json::to_string(&msg) {
                let actix_msg = ServerMessage(json_string);
                for pid in &game.player_ids {
                    if let Some(addr) = self.sessions.get(pid) {
                        let _ = addr.do_send(actix_msg.clone());
                    }
                }
            }
        }
    }

    pub fn broadcast_lobby_status(&self) {
        info!("Broadcasting lobby status to all players");
        let games_data: Vec<(String, usize, usize)> = self.games.iter()
            .map(|(gid, game)| (gid.clone(), game.player_ids.len(), game.max_players))
            .collect();

        let lobby_update = shared::ServerMessage::LobbyUpdate { games: games_data };
        for pid in self.sessions.keys() {
            self.send_server_msg(*pid, lobby_update.clone());
        }
    }
    fn send_game_started(&self, pid: Uuid, game_id: &str) {
        let Some(game) = self.games.get(game_id) else { return };

        let players: Vec<shared::PlayerInfo> = game.player_ids.iter()
            .filter_map(|&id| game.turn_manager.get_player_info(id))
            .map(|p| p.into())
            .collect();

        let board = shared::BoardState {
            hexes: game.turn_manager.board.hexes.iter().map(|(c, h)| shared::HexInfo {
                q: c.0, r: c.1, resource: h.resource, number: h.number
            }).collect(),
            settlements: self.get_buildings(&game, true),
            cities: self.get_buildings(&game, false),
            roads: game.turn_manager.board.edges.iter()
                .filter_map(|(c, e)| e.owner.map(|owner| shared::BuildingInfo { player_id: owner, x: c.0, y: c.1 }))
                .collect(),
            robber_pos: game.turn_manager.robber.pos,
            ports: game.turn_manager.board.unique_ports().iter().map(|p| p.into()).collect(),
        };

        let msg = shared::ServerMessage::GameStarted {
            your_player_id: pid,
            players,
            board,
            game_phase: game.phase.clone(),
        };

        self.send_server_msg(pid, msg);
    }
    pub(crate) fn send_full_sync(&mut self, pid: Uuid, game_id: &str) {
        let Some(game) = self.games.get(game_id) else { return };

        let last_roll = game.turn_manager.last_roll.map(|total| {
            let d1 = total / 2;
            (d1, total - d1)
        });

        let sync_msg = shared::ServerMessage::FullStateSync {
            player_id: pid,
            players: game.get_all_players_info(),
            board: game.turn_manager.board.to_info(game.turn_manager.robber.pos),
            game_phase: game.phase.clone(),
            current_turn_player_id: game.turn_manager.current_player_id(),
            robber_pos: game.turn_manager.robber.pos,
            last_dice_roll: last_roll,
        };

        self.send_server_msg(pid, sync_msg);
        if game.turn_manager.player_secret_victory_points(pid) != 0 {
            self.send_secret_victory_points_to_player(pid, &game_id.to_string());
        }

        if game.seven_roller == Some(pid) {
            self.send_server_msg(pid, MustMoveRobber { player_id: pid });
        }
    }
    pub(crate) fn send_error(&self, pid: Uuid, error_msg: &str) {
        self.send_server_msg(pid, shared::ServerMessage::Error {
            message: error_msg.to_string(),
        });
    }
    pub(crate) fn broadcast_resource_updates(&mut self, gid: &str) {
    let Some(game) = self.games.get(gid) else { return };

    for &pid in &game.player_ids {
        if let Some(player) = game.turn_manager.get_player_info(pid) {
            let res: shared::Resources = (&player.resources).into();
            info!("Sending resource update to pid {}: {:?}", pid, res);
            let msg = shared::ServerMessage::ResourceUpdate {
                player_id: pid,
                resources: res,
            };
            self.send_server_msg(pid, msg);
        }
        }
    }

    pub(crate) fn broadcast_players_update(&self, gid: &str) {
        if let Some(game) = self.games.get(gid) {
            let players: Vec<shared::PlayerInfo> = game.player_ids.iter()
                .filter_map(|&pid| game.turn_manager.get_player_info(pid).map(|p| (pid, p)))
                .map(|(pid, p)| {
                    let mut info = shared::PlayerInfo::from(p);
                    info.has_longest_road = game.turn_manager.road_bonus.holder() == Some(pid);
                    info.has_largest_army = game.turn_manager.army_bonus.holder() == Some(pid);
                    info
                })
                .collect();
            let msg = shared::ServerMessage::PlayersUpdate { players };
            self.broadcast_to_game(gid, msg);
        }
    }
    pub fn send_secret_victory_points_to_player(&self, pid: Uuid, gid: &String) {
        if let Some(game) = self.games.get(gid) {
            let points = game.turn_manager.player_secret_victory_points(pid);
            let msg = shared::ServerMessage::PlayerSecretVictoryPointsUpdated {
                secret_victory_points: points as i32,
            };
            self.send_server_msg(pid, msg);
        } else {
            error!("Game not found for UUID: {}", gid);
        }
    }
    pub fn save_game_async(&self, gid: &str, ctx: &mut Context<Self>) {
        if let Some(instance) = self.games.get(gid).cloned() {
            let repo = self.repo.clone();
            ctx.spawn(async move {
                let _ = repo.save_game_instance(&instance).await;
            }.into_actor(self));
        }
    }
    pub fn refresh_game_for_all(&mut self, gid: &str) {
        let player_ids: Vec<Uuid> = self.games.get(gid)
            .map(|g| g.player_ids.clone())
            .unwrap_or_default();

        for pid in player_ids {
            self.send_game_started(pid, gid);
        }
    }
    fn get_buildings(&self, game: &GameInstance, get_settlements: bool) -> Vec<shared::BuildingInfo> {
        game.turn_manager.board.vertices.iter()
            .filter_map(|(coord, vertex)| {
                let building = vertex.building.as_ref()?;
                let owner = vertex.owner?;
                let is_city = matches!(building, VertexBuilding::City);
                if get_settlements != is_city {
                    Some(shared::BuildingInfo {
                        player_id: owner,
                        x: coord.0,
                        y: coord.1,
                    })
                } else {
                    None
                }
            })
            .collect()
    }

}