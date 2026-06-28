use super::{Lobby, actions};
use crate::network::message::{ClientActorMessage, Connect, Disconnect};
use actions::handle_game_request;
use actix::prelude::*;
use log::{error, info};
use shared::ServerMessage::MustMoveRobber;
use shared::{ClientRequest, GamePhase, ServerMessage};
use std::collections::HashSet;

impl Handler<Connect> for Lobby {
    type Result = ();
    fn handle(&mut self, msg: Connect, _: &mut Context<Self>) {
        let pid = msg.player_id;
        info!("Player {} connected via WebSocket", pid);
        self.sessions.insert(pid, msg.addr);
        if let Some(game_id) = self.player_to_game.get(&pid).cloned() {
            if self.games.contains_key(&game_id) {
                if self.games.get(&game_id).unwrap().turn_manager.game_over {
                    error!(
                        "Player {} attempted to reconnect to game {} which is over",
                        pid, game_id
                    );
                    return;
                }
                info!(
                    "Player {} recognized in game {}. Restoring state...",
                    pid, game_id
                );
                self.send_server_msg(
                    pid,
                    ServerMessage::Joined {
                        player_id: pid,
                        game_id: game_id.clone(),
                    },
                );
                self.send_full_sync(pid, &game_id);
                return;
            }
        }
        self.broadcast_lobby_status();
    }
}

impl Handler<Disconnect> for Lobby {
    type Result = ();
    fn handle(&mut self, msg: Disconnect, _: &mut Context<Self>) {
        info!("Player {} disconnected", msg.player_id);
        self.sessions.remove(&msg.player_id);
        self.broadcast_lobby_status();
    }
}

impl Handler<ClientActorMessage> for Lobby {
    type Result = ();
    fn handle(&mut self, msg: ClientActorMessage, ctx: &mut Context<Self>) {
        let pid = msg.player_id;

        match msg.req {
            ClientRequest::CreateGame { .. }
            | ClientRequest::JoinGame { .. }
            | ClientRequest::GetLobbyList | ClientRequest::LeaveGame {..} => {
                actions::handle_lobby_action(self, pid, msg.req, ctx);
                return;
            }
            _ => {}
        }

        let gid = match self.player_to_game.get(&pid) {
            Some(id) => id.clone(),
            None => return self.send_error(pid, "You are not in a game"),
        };
        if self.games.get_mut(&gid).unwrap().turn_manager.game_over {
            return self.send_error(pid, "Game is already over");
        }

        let action_result = if let Some(game) = self.games.get_mut(&gid) {
            handle_game_request(pid, msg.req, game)
        } else {
            Err("Game not found".to_string())
        };

        match action_result {
            Ok(msg) => {
                let game = self.games.get_mut(&gid).unwrap();
                if game.turn_manager.has_player_won(pid) {
                    let victory_msg = ServerMessage::PlayerWon {
                        player_id: pid,
                        secret_victory_points: game.turn_manager.player_secret_victory_points(pid),
                    };
                    self.broadcast_to_game(&gid, victory_msg);
                self.games.get_mut(&gid).unwrap().turn_manager.game_over = true;
                }
                let repo = self.repo.clone();
                if let Some(instance) = self.games.get(&gid).cloned() {
                    ctx.spawn(
                        async move {
                            let _ = repo.save_game_instance(&instance).await;
                        }
                        .into_actor(self),
                    );
                }
                self.broadcast_to_game(&gid, msg.clone());
                let mut players_must_discard = Vec::new();

                if let ServerMessage::DiceRolled { dice_1, dice_2, .. } = &msg {
                    if (dice_1 + dice_2) == 7 {
                        if let Some(game) = self.games.get(&gid) {
                            for &p_id in &game.pending_discards {
                                if let Some(player) = game.turn_manager.bank.players.get(&p_id) {
                                    let total = player.resources.clone().get_cards_total();

                                    if total > 7 {
                                        players_must_discard.push((p_id, (total / 2) as usize));
                                    }
                                }
                            }
                        }
                        info!("7 rolled! Sending ResourceUpdate BEFORE MustDiscardCards");
                        if players_must_discard.is_empty() {
                            self.send_server_msg(pid, MustMoveRobber { player_id: pid })
                        }
                        self.broadcast_resource_updates(&gid);
                        self.broadcast_players_update(&gid);
                    }
                }

                let resource_update_needed = matches!(msg,
                    ServerMessage::DiceRolled{..} | ServerMessage::Built{..} |
                    ServerMessage::CardsDiscarded{..} | ServerMessage::BankTradeCompleted{..} |
                    ServerMessage::TradeCompleted{..} | ServerMessage::PlayerRobbed{..} |
                    ServerMessage::DevCardBought{..} | ServerMessage::DevCardPlayed{..} |
                    ServerMessage::YearOfPlentyResourcesReceived{..} |
                    ServerMessage::MonopolyResourcesStolen{..}
                );

                if resource_update_needed {
                    self.broadcast_resource_updates(&gid);
                    self.broadcast_players_update(&gid);
                }

                if matches!(&msg, ServerMessage::Built { structure_type, .. } if structure_type == "SETTLEMENT" || structure_type == "CITY")
                {
                    self.broadcast_players_update(&gid);
                }

                if let ServerMessage::Built { structure_type, .. } = &msg {
                    if structure_type == "ROAD" {
                        let phase_check = self
                            .games
                            .get(&gid)
                            .map(|g| {
                                matches!(
                                    g.phase,
                                    GamePhase::InitialPlacementRound1
                                        | GamePhase::InitialPlacementRound2
                                )
                            })
                            .unwrap_or(false);
                        if phase_check {
                            let transitioned = self.advance_initial_placement(&gid);
                            if transitioned {
                                let p_msg = ServerMessage::PhaseChanged {
                                    new_phase: GamePhase::RegularPlay,
                                };
                                self.broadcast_to_game(&gid, p_msg);

                            }
                            if let Some(game) = self.games.get(&gid) {
                                let n_msg = ServerMessage::NextTurn {
                                    player_id: game.turn_manager.current_player_id,
                                };
                                self.broadcast_to_game(&gid, n_msg);
                            }
                        }

                        // Send MustPlaceRoads if there are still free roads remaining from Road Builder
                        if let Some(game) = self.games.get(&gid) {
                            if game.free_roads_remaining > 0 {
                                self.send_server_msg(pid, ServerMessage::MustPlaceRoads {
                                    player_id: pid,
                                    roads_remaining: game.free_roads_remaining,
                                });
                            }
                        }
                    }
                }

                for (conn_id, count) in players_must_discard {
                    self.send_server_msg(
                        conn_id,
                        ServerMessage::MustDiscardCards {
                            player_id: conn_id,
                            count,
                        },
                    );
                }

                if matches!(msg, ServerMessage::CardsDiscarded { .. }) {
                    if let Some(game) = self.games.get(&gid) {
                        if game.pending_discards.is_empty() && game.seven_roller.is_some() {
                            let roller = game.seven_roller.unwrap();
                            self.send_server_msg(
                                roller,
                                MustMoveRobber { player_id: roller },
                            );
                        }
                    }
                }

                // Send MustMoveRobber when a knight card is played
                if let ServerMessage::DevCardPlayed {
                    card_type: shared::DevCardType::Knight,
                    ..
                } = &msg
                {
                    if let Some(game) = self.games.get(&gid) {
                        if let Some(knight_player) = game.knight_mover {
                            self.send_server_msg(
                                knight_player,
                                MustMoveRobber {
                                    player_id: knight_player,
                                },
                            );
                        }
                    }
                }
                // Victory Point cards auto-grant on purchase, not play
                if let ServerMessage::DevCardBought { card_type: shared::DevCardType::VictoryPoint, .. } = &msg {
                    self.send_secret_victory_points_to_player(pid, &gid);
                }

                // Send MustPlaceRoads when a Road Builder card is played
                if let ServerMessage::DevCardPlayed { card_type: shared::DevCardType::RoadBuilding, .. } = &msg {
                    if let Some(game) = self.games.get(&gid) {
                        if game.free_roads_remaining > 0 {
                            self.send_server_msg(pid, ServerMessage::MustPlaceRoads {
                                player_id: pid,
                                roads_remaining: game.free_roads_remaining,
                            });
                        }
                    }
                }

                // Send MustChooseYearOfPlentyResources when Year of Plenty card is played
                if let ServerMessage::DevCardPlayed { card_type: shared::DevCardType::YearOfPlenty, .. } = &msg {
                    if let Some(game) = self.games.get(&gid) {
                        if game.year_of_plenty_pending.is_some() {
                            self.send_server_msg(pid, ServerMessage::MustChooseYearOfPlentyResources {
                                player_id: pid,
                            });
                        }
                    }
                }

                // Send MustChooseMonopolyResource when Monopoly card is played
                if let ServerMessage::DevCardPlayed { card_type: shared::DevCardType::Monopoly, .. } = &msg {
                    if let Some(game) = self.games.get(&gid) {
                        if game.monopoly_pending.is_some() {
                            self.send_server_msg(pid, ServerMessage::MustChooseMonopolyResource {
                                player_id: pid,
                            });
                        }
                    }
                }

                // G. Can Rob Players (After MoveRobber)
                if let ServerMessage::RobberMoved { new_q, new_r, .. } = &msg {
                    if let Some(game) = self.games.get(&gid) {
                        let mut robbable_players = HashSet::new();
                        let adjacent_vertices =
                            crate::game::entities::board::Board::get_adjacent_vertices((
                                *new_q, *new_r,
                            ));
                        for vertex_coord in &adjacent_vertices {
                            if let Some(vertex) = game.turn_manager.board.vertices.get(vertex_coord)
                            {
                                if vertex.building.is_some() {
                                    if let Some(owner_id) = vertex.owner {
                                        // Don't rob yourself
                                        if owner_id != pid {
                                            robbable_players.insert(owner_id);
                                        }
                                    }
                                }
                            }
                        }

                        self.send_server_msg(
                            pid,
                            ServerMessage::CanRobPlayers {
                                player_ids: robbable_players.into_iter().collect(),
                            },
                        );
                    }
                }
            }
            Err(e) => self.send_error(pid, &e),
        }
    }
}