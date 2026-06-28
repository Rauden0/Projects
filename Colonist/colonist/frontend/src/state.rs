use leptos::*;
use shared::{ClientRequest, ServerMessage, Resources, PlayerInfo, HexInfo, BuildingInfo, GamePhase, PortInfo};
use gloo_net::websocket::futures::WebSocket;
use gloo_net::websocket::Message;
use futures::{SinkExt, StreamExt};
use futures::channel::mpsc::UnboundedSender;
use uuid::Uuid;

#[derive(Clone, Debug, PartialEq)]
pub enum BuildMode {
    None,
    Settlement,
    City,
    Road,
}

/// A pending trade offer from another player
#[derive(Clone, Debug, PartialEq)]
pub struct PendingTradeOffer {
    pub offer_id: u64,
    pub proposer_id: Uuid,
    pub target_player_id: Option<Uuid>,
    pub offering: Resources,
    pub requesting: Resources,
    pub received_at: f64,  // Timestamp when we received this trade (for timer)
}

#[derive(Clone, Debug, Copy)]
pub struct GameState {
    // Lobby state
    pub messages: RwSignal<Vec<String>>,
    pub is_in_game: RwSignal<bool>,
    pub lobby_games: RwSignal<Vec<(String, usize, usize)>>,
    pub ws_sender: RwSignal<Option<UnboundedSender<Message>>>,
    pub connection_error: RwSignal<Option<String>>,
    pub is_connecting: RwSignal<bool>,

    // Game state
    pub game_id: RwSignal<Option<String>>,
    pub player_id: RwSignal<Option<Uuid>>,
    pub players: RwSignal<Vec<PlayerInfo>>,
    pub current_turn_player: RwSignal<Uuid>,
    pub my_resources: RwSignal<Resources>,
    pub game_phase: RwSignal<GamePhase>,

    // Board state
    pub hexes: RwSignal<Vec<HexInfo>>,
    pub settlements: RwSignal<Vec<BuildingInfo>>,
    pub cities: RwSignal<Vec<BuildingInfo>>,
    pub roads: RwSignal<Vec<BuildingInfo>>,
    pub robber_pos: RwSignal<Option<(i32, i32)>>,
    pub board_ports: RwSignal<Vec<PortInfo>>,

    // UI state
    pub last_dice_roll: RwSignal<Option<(u8, u8)>>,
    pub build_mode: RwSignal<BuildMode>,
    pub my_dev_cards: RwSignal<Vec<shared::DevCardType>>,

    // Robber state
    pub must_discard_count: RwSignal<Option<usize>>,
    pub must_move_robber: RwSignal<bool>,
    pub must_steal_from_players: RwSignal<Vec<Uuid>>,
    pub waiting_for_discards: RwSignal<bool>,  // True when I rolled 7 and waiting for others to discard

    // Road Builder state
    pub free_roads_remaining: RwSignal<u8>,  // Free roads from Road Builder card

    // Year of Plenty state
    pub year_of_plenty_pending: RwSignal<bool>,  // True when player must choose 2 resources

    // Monopoly state
    pub monopoly_pending: RwSignal<bool>,  // True when player must choose a resource type

    // Port trading state
    pub my_ports: RwSignal<Vec<shared::PortType>>,

    // Player-to-player trading state
    pub incoming_trades: RwSignal<Vec<PendingTradeOffer>>,
    pub my_pending_trade: RwSignal<Option<u64>>,  // Trade ID of trade I proposed

    // Winner
    pub secret_victory_points:RwSignal<i32>,
    pub winner_player_id: RwSignal<Option<Uuid>>,
}

impl GameState {
    pub fn send(&self, req: ClientRequest) {
        if let Some(tx) = self.ws_sender.get_untracked() {
            if let Ok(json) = serde_json::to_string(&req) {
                logging::log!("Sending: {}", json);
                let _ = tx.unbounded_send(Message::Text(json));
            }
        } else {
            logging::error!("Cannot send: WebSocket not connected!");
        }
    }

    pub fn process_message(&self, text: String) {
        if let Ok(msg) = serde_json::from_str::<ServerMessage>(&text) {
            match msg {
                ServerMessage::LobbyUpdate { games } => {
                    logging::log!("Lobbies updated: {:?}", games);
                    self.lobby_games.set(games);
                }
                ServerMessage::Joined { game_id, .. } => {
                    self.game_id.set(Some(game_id));
                    self.is_in_game.set(true);
                }
                ServerMessage::PlayersUpdate { players: updated_players } => {
                    logging::log!("Players update received");
                    let my_id = self.player_id.get_untracked();
                    self.players.update(|players| {
                        for updated_player in updated_players {
                            if let Some(player) = players.iter_mut().find(|p| p.player_id == updated_player.player_id) {
                                // Update my_ports if this is the current player
                                if Some(updated_player.player_id) == my_id {
                                    self.my_ports.set(updated_player.ports.clone());
                                }
                                *player = updated_player;
                            }
                        }
                    });
                }
                ServerMessage::GameStarted { your_player_id, players, board, game_phase } => {
                    logging::log!("Game started! Your ID: {}, Phase: {:?}", your_player_id, game_phase);
                    self.player_id.set(Some(your_player_id));
                    self.players.set(players.clone());
                    self.hexes.set(board.hexes);
                    self.settlements.set(board.settlements);
                    self.cities.set(board.cities);
                    self.roads.set(board.roads);
                    self.robber_pos.set(Some(board.robber_pos));
                    self.board_ports.set(board.ports);
                    self.game_phase.set(game_phase.clone());

                    // Find my resources, dev cards, and ports from the players list
                    if let Some(my_player) = players.iter().find(|p| p.player_id == your_player_id) {
                        self.current_turn_player.set(players.first().map(|p| p.player_id).unwrap_or(Uuid::nil()));

                        // DEBUG LOGGING: Verify each player has unique ID
                        logging::log!("=== DEBUG: Player List ===");
                        for (idx, p) in players.iter().enumerate() {
                            logging::log!("  Player {}: ID={}, Name={}", idx, p.player_id, p.name);
                        }
                        logging::log!("  Your ID: {}", your_player_id);
                        logging::log!("========================");

                        // Initial resources are empty, will be updated via ResourceUpdate
                        self.my_dev_cards.set(my_player.dev_cards.clone());
                        self.my_ports.set(my_player.ports.clone());
                    }

                    let phase_msg = match game_phase {
                        GamePhase::InitialPlacementRound1 => "Starting initial placement - Round 1!",
                        GamePhase::InitialPlacementRound2 => "Initial placement - Round 2!",
                        GamePhase::RegularPlay => "Game started!",
                        GamePhase::WaitingForPlayers => "Waiting for players...",
                    };
                    self.messages.update(|m| m.push(format!("{} ({} players)", phase_msg, players.len())));
                }
                ServerMessage::DiceRolled { player_id, dice_1, dice_2, discards_pending } => {
                    let total = dice_1 + dice_2;
                    logging::log!("Player {} rolled: {} + {} = {}, discards_pending: {}", player_id, dice_1, dice_2, total, discards_pending);
                    self.last_dice_roll.set(Some((dice_1, dice_2)));

                    // If I rolled a 7 and others need to discard, show waiting message
                    if total == 7 && Some(player_id) == self.player_id.get_untracked() && discards_pending > 0 {
                        self.waiting_for_discards.set(true);
                        logging::log!("I rolled 7! {} players must discard...", discards_pending);
                        self.messages.update(|m| m.push(format!(
                            "You rolled 7! Waiting for {} player{} to discard...",
                            discards_pending,
                            if discards_pending == 1 { "" } else { "s" }
                        )));
                    }

                    let player_name = self.players.get_untracked()
                        .iter()
                        .find(|p| p.player_id == player_id)
                        .map(|p| p.name.clone())
                        .unwrap_or_else(|| format!("Player {}", player_id));

                    self.messages.update(|m| m.push(format!("{} rolled {} ({}+{})", player_name, total, dice_1, dice_2)));
                }
                ServerMessage::ResourceUpdate { player_id, resources } => {
                    logging::log!("Resource update for player {}: {:?}", player_id, resources);

                    // Update my resources if it's me
                    if Some(player_id) == self.player_id.get_untracked() {
                        self.my_resources.set(resources.clone());
                    }

                    // Update resources in players vector
                    self.players.update(|players| {
                        if let Some(player) = players.iter_mut().find(|p| p.player_id == player_id) {
                            player.resources = resources;
                        }
                    });
                }
                ServerMessage::Built { player_id, structure_type, coords } => {
                    logging::log!("Player {} built {} at {:?}", player_id, structure_type, coords);

                    let building = BuildingInfo {
                        player_id,
                        x: coords[0],
                        y: coords[1],
                    };

                    match structure_type.as_str() {
                        "SETTLEMENT" => self.settlements.update(|s| s.push(building)),
                        "CITY" => self.cities.update(|c| c.push(building)),
                        "ROAD" => {
                            self.roads.update(|r| r.push(building));
                            // If it's me and I was using free roads, decrement the counter
                            if Some(player_id) == self.player_id.get_untracked() {
                                let remaining = self.free_roads_remaining.get_untracked();
                                if remaining > 0 {
                                    let new_remaining = remaining - 1;
                                    self.free_roads_remaining.set(new_remaining);
                                    // If no more free roads, clear build mode
                                    if new_remaining == 0 {
                                        self.build_mode.set(BuildMode::None);
                                    }
                                }
                            }
                        }
                        _ => {}
                    }

                    let player_name = self.players.get_untracked()
                        .iter()
                        .find(|p| p.player_id == player_id)
                        .map(|p| p.name.clone())
                        .unwrap_or_else(|| format!("Player {}", player_id));

                    self.messages.update(|m| m.push(format!("{} built a {}", player_name, structure_type.to_lowercase())));
                }
                ServerMessage::NextTurn { player_id } => {
                    logging::log!("Next turn: Player {}", player_id);
                    self.current_turn_player.set(player_id);
                    self.last_dice_roll.set(None); // Clear dice roll for new turn
                    self.waiting_for_discards.set(false); // Clear waiting state on turn change

                    let player_name = self.players.get_untracked()
                        .iter()
                        .find(|p| p.player_id == player_id)
                        .map(|p| p.name.clone())
                        .unwrap_or_else(|| format!("Player {}", player_id));

                    self.messages.update(|m| m.push(format!("It's {}'s turn", player_name)));
                }
                ServerMessage::PhaseChanged { new_phase } => {
                    logging::log!("Phase changed to: {:?}", new_phase);
                    self.game_phase.set(new_phase.clone());

                    let phase_msg = match new_phase {
                        GamePhase::InitialPlacementRound1 => "Initial placement - Round 1",
                        GamePhase::InitialPlacementRound2 => "Initial placement - Round 2",
                        GamePhase::RegularPlay => "Regular play started!",
                        GamePhase::WaitingForPlayers => "Waiting for players",
                    };
                    self.messages.update(|m| m.push(phase_msg.to_string()));
                }
                ServerMessage::ChatMessage { player_id, text } => {
                    logging::log!("Chat from player {}: {}", player_id, text);

                    let player_name = self.players.get_untracked()
                        .iter()
                        .find(|p| p.player_id == player_id)
                        .map(|p| p.name.clone())
                        .unwrap_or_else(|| format!("Player {}", player_id));

                    self.messages.update(|m| m.push(format!("{}: {}", player_name, text)));
                }
                ServerMessage::DevCardBought { player_id, card_type } => {
                    logging::log!("Player {} bought dev card: {:?}", player_id, card_type);

                    // If it's me, add to my cards
                    if Some(player_id) == self.player_id.get_untracked() {
                        self.my_dev_cards.update(|cards| cards.push(card_type.clone()));
                    }

                    // Update player's dev_cards in the players list
                    self.players.update(|players| {
                        if let Some(player) = players.iter_mut().find(|p| p.player_id == player_id) {
                            player.dev_cards.push(card_type.clone());
                        }
                    });

                    let player_name = self.players.get_untracked()
                        .iter()
                        .find(|p| p.player_id == player_id)
                        .map(|p| p.name.clone())
                        .unwrap_or_else(|| format!("Player {}", player_id));

                    let card_name = match card_type {
                        shared::DevCardType::Knight => "Knight",
                        shared::DevCardType::VictoryPoint => "Victory Point",
                        shared::DevCardType::RoadBuilding => "Road Building",
                        shared::DevCardType::Monopoly => "Monopoly",
                        shared::DevCardType::YearOfPlenty => "Year of Plenty",
                    };

                    self.messages.update(|m| m.push(format!("{} bought a {} card", player_name, card_name)));
                }
                ServerMessage::DevCardPlayed { player_id, card_type } => {
                    logging::log!("Player {} played dev card: {:?}", player_id, card_type);

                    // If it's me, remove from my cards
                    if Some(player_id) == self.player_id.get_untracked() {
                        self.my_dev_cards.update(|cards| {
                            if let Some(pos) = cards.iter().position(|c| c == &card_type) {
                                cards.remove(pos);
                            }
                        });
                    }

                    // Update player's dev_cards in the players list
                    self.players.update(|players| {
                        if let Some(player) = players.iter_mut().find(|p| p.player_id == player_id) {
                            if let Some(pos) = player.dev_cards.iter().position(|c| c == &card_type) {
                                player.dev_cards.remove(pos);
                            }
                        }
                    });

                    let player_name = self.players.get_untracked()
                        .iter()
                        .find(|p| p.player_id == player_id)
                        .map(|p| p.name.clone())
                        .unwrap_or_else(|| format!("Player {}", player_id));

                    let card_name = match card_type {
                        shared::DevCardType::Knight => "Knight",
                        shared::DevCardType::VictoryPoint => "Victory Point",
                        shared::DevCardType::RoadBuilding => "Road Building",
                        shared::DevCardType::Monopoly => "Monopoly",
                        shared::DevCardType::YearOfPlenty => "Year of Plenty",
                    };

                    self.messages.update(|m| m.push(format!("{} played a {} card", player_name, card_name)));
                }
                ServerMessage::Error { message } => {
                    logging::error!("Server error: {}", message);
                    self.messages.update(|m| m.push(format!("Error: {}", message)));
                }
                ServerMessage::RobberMoved { player_id, new_q, new_r } => {
                    logging::log!("Player {} moved robber to ({}, {})", player_id, new_q, new_r);
                    self.robber_pos.set(Some((new_q, new_r)));

                    let player_name = self.players.get_untracked()
                        .iter()
                        .find(|p| p.player_id == player_id)
                        .map(|p| p.name.clone())
                        .unwrap_or_else(|| format!("Player {}", player_id));

                    self.messages.update(|m| m.push(format!("{} moved the robber", player_name)));
                }
                ServerMessage::PlayerRobbed { thief_id, victim_id, resource } => {
                    logging::log!("Player {} robbed from Player {}", thief_id, victim_id);

                    let thief_name = self.players.get_untracked()
                        .iter()
                        .find(|p| p.player_id == thief_id)
                        .map(|p| p.name.clone())
                        .unwrap_or_else(|| format!("Player {}", thief_id));

                    let victim_name = self.players.get_untracked()
                        .iter()
                        .find(|p| p.player_id == victim_id)
                        .map(|p| p.name.clone())
                        .unwrap_or_else(|| format!("Player {}", victim_id));

                    if let Some(res) = resource {
                        let res_name = format!("{:?}", res);
                        self.messages.update(|m| m.push(format!("{} stole {} from {}", thief_name, res_name, victim_name)));
                    } else {
                        self.messages.update(|m| m.push(format!("{} couldn't rob {} (no cards)", thief_name, victim_name)));
                    }
                }
                ServerMessage::MustDiscardCards { player_id, count } => {
                    logging::log!("Player {} must discard {} cards", player_id, count);

                    let player_name = self.players.get_untracked()
                        .iter()
                        .find(|p| p.player_id == player_id)
                        .map(|p| p.name.clone())
                        .unwrap_or_else(|| format!("Player {}", player_id));

                    self.messages.update(|m| m.push(format!("{} must discard {} cards", player_name, count)));

                    // Show discard UI if it's me
                    if Some(player_id) == self.player_id.get_untracked() {
                        self.must_discard_count.set(Some(count));
                    }
                }
                ServerMessage::CardsDiscarded { player_id, count } => {
                    logging::log!("✅ Player {} discarded {} cards", player_id, count);

                    let player_name = self.players.get_untracked()
                        .iter()
                        .find(|p| p.player_id == player_id)
                        .map(|p| p.name.clone())
                        .unwrap_or_else(|| format!("Player {}", player_id));

                    self.messages.update(|m| m.push(format!("{} discarded {} cards", player_name, count)));

                    // If this is MY discard confirmation, close the discard modal
                    if Some(player_id) == self.player_id.get_untracked() {
                        logging::log!("✅ My discard confirmed! Closing modal.");
                        self.must_discard_count.set(None);
                    }
                }
                ServerMessage::MustMoveRobber { player_id } => {
                    logging::log!("Player {} must move robber", player_id);

                    // Show robber movement UI if it's me
                    if Some(player_id) == self.player_id.get_untracked() {
                        self.waiting_for_discards.set(false);  // Done waiting, now can move robber
                        self.must_move_robber.set(true);
                        self.messages.update(|m| m.push("You must move the robber!".to_string()));
                    }
                }
                ServerMessage::CanRobPlayers { player_ids } => {
                    logging::log!("Can rob from players: {:?}", player_ids);

                    if player_ids.is_empty() {
                        self.messages.update(|m| m.push("No players to rob".to_string()));
                    } else {
                        self.must_steal_from_players.set(player_ids);
                    }
                }
                ServerMessage::MustPlaceRoads { player_id, roads_remaining } => {
                    logging::log!("Player {} must place {} roads", player_id, roads_remaining);

                    // If it's me, enter road building mode
                    if Some(player_id) == self.player_id.get_untracked() {
                        self.free_roads_remaining.set(roads_remaining);
                        self.build_mode.set(BuildMode::Road);
                        self.messages.update(|m| m.push(format!(
                            "Place {} free road{}!",
                            roads_remaining,
                            if roads_remaining == 1 { "" } else { "s" }
                        )));
                    }
                }
                ServerMessage::MustChooseYearOfPlentyResources { player_id } => {
                    logging::log!("Player {} must choose 2 resources for Year of Plenty", player_id);

                    // If it's me, show the resource picker
                    if Some(player_id) == self.player_id.get_untracked() {
                        self.year_of_plenty_pending.set(true);
                        self.messages.update(|m| m.push("Choose 2 resources to receive from the bank!".to_string()));
                    }
                }
                ServerMessage::YearOfPlentyResourcesReceived { player_id, resource1, resource2 } => {
                    logging::log!("Player {} received {:?} and {:?} from Year of Plenty", player_id, resource1, resource2);

                    let me = self.player_id.get_untracked();

                    if Some(player_id) == me {
                        self.year_of_plenty_pending.set(false);
                
                        self.players.update(|players| {
                            if let Some(p) = players.iter_mut().find(|p| p.player_id == player_id) {
                                match resource1 {
                                    shared::ResourceType::Brick => p.resources.brick += 1,
                                    shared::ResourceType::Wood => p.resources.lumber += 1,
                                    shared::ResourceType::Sheep | shared::ResourceType::Wool => p.resources.wool += 1,
                                    shared::ResourceType::Wheat => p.resources.grain += 1,
                                    shared::ResourceType::Ore => p.resources.ore += 1,
                                    shared::ResourceType::Desert => {}
                                }
                                match resource2 {
                                    shared::ResourceType::Brick => p.resources.brick += 1,
                                    shared::ResourceType::Wood => p.resources.lumber += 1,
                                    shared::ResourceType::Sheep | shared::ResourceType::Wool => p.resources.wool += 1,
                                    shared::ResourceType::Wheat => p.resources.grain += 1,
                                    shared::ResourceType::Ore => p.resources.ore += 1,
                                    shared::ResourceType::Desert => {}
                                }
                            }
                        });
                    }
                
                    let player_name = self.players.get_untracked()
                        .iter()
                        .find(|p| p.player_id == player_id)
                        .map(|p| p.name.clone())
                        .unwrap_or_else(|| "Someone".to_string());
                
                    self.messages.update(|m| m.push(format!(
                        "{} received {:?} and {:?} from Year of Plenty.",
                        player_name, resource1, resource2
                    )));
                }
                ServerMessage::MustChooseMonopolyResource { player_id } => {
                    logging::log!("Player {} must choose a resource for Monopoly", player_id);

                    // If it's me, show the resource picker
                    if Some(player_id) == self.player_id.get_untracked() {
                        self.monopoly_pending.set(true);
                        self.messages.update(|m| m.push("Choose a resource type to steal from all players!".to_string()));
                    }
                }
                ServerMessage::MonopolyResourcesStolen { player_id, resource, total_stolen } => {
                    logging::log!("Player {} stole {} {:?} via Monopoly", player_id, total_stolen, resource);

                    // If it's me, close the modal
                    if Some(player_id) == self.player_id.get_untracked() {
                        self.monopoly_pending.set(false);
                    }

                    let player_name = self.players.get_untracked()
                        .iter()
                        .find(|p| p.player_id == player_id)
                        .map(|p| p.name.clone())
                        .unwrap_or_else(|| "Someone".to_string());

                    self.messages.update(|m| m.push(format!(
                        "{} used Monopoly and stole {} {:?} from other players!",
                        player_name, total_stolen, resource
                    )));
                }
                ServerMessage::BankTradeCompleted { player_id, gave, received } => {
                    logging::log!("Player {} traded with bank: gave {:?}, received {:?}", player_id, gave, received);

                    let player_name = self.players.get_untracked()
                        .iter()
                        .find(|p| p.player_id == player_id)
                        .map(|p| p.name.clone())
                        .unwrap_or_else(|| format!("Player {}", player_id));

                    // Calculate the ratio used based on player's ports
                    let ratio = self.get_best_ratio(gave);

                    let gave_name = match gave {
                        shared::ResourceType::Brick => "Brick",
                        shared::ResourceType::Wood => "Wood",
                        shared::ResourceType::Wool | shared::ResourceType::Sheep => "Sheep",
                        shared::ResourceType::Wheat => "Wheat",
                        shared::ResourceType::Ore => "Ore",
                        shared::ResourceType::Desert => "Desert",
                    };

                    let received_name = match received {
                        shared::ResourceType::Brick => "Brick",
                        shared::ResourceType::Wood => "Wood",
                        shared::ResourceType::Wool | shared::ResourceType::Sheep => "Sheep",
                        shared::ResourceType::Wheat => "Wheat",
                        shared::ResourceType::Ore => "Ore",
                        shared::ResourceType::Desert => "Desert",
                    };

                    self.messages.update(|m| m.push(format!("{} traded {} {} for 1 {}", player_name, ratio, gave_name, received_name)));
                }
                ServerMessage::PortsUpdate { player_id, ports } => {
                    logging::log!("Player {} ports updated: {:?}", player_id, ports);

                    // If it's me, update my ports
                    if Some(player_id) == self.player_id.get_untracked() {
                        self.my_ports.set(ports.clone());

                        // Log what port was gained
                        if let Some(new_port) = ports.last() {
                            let port_desc = match new_port {
                                shared::PortType::ThreeToOne => "3:1 port".to_string(),
                                shared::PortType::TwoToOne(res) => format!("2:1 {:?} port", res),
                            };
                            self.messages.update(|m| m.push(format!("You gained access to a {}!", port_desc)));
                        }
                    }
                }
                ServerMessage::TradeProposed { offer_id, proposer_id, target_player_id, offering, requesting } => {
                    logging::log!("Trade proposed: {} from player {}", offer_id, proposer_id);

                    let my_id = self.player_id.get_untracked();

                    // If I'm the proposer, track my trade
                    if Some(proposer_id) == my_id {
                        self.my_pending_trade.set(Some(offer_id));
                    }

                    // If this trade is for me (or open to everyone) and I'm not the proposer, add to incoming
                    let is_for_me = target_player_id.is_none() || target_player_id == my_id;
                    if is_for_me && Some(proposer_id) != my_id {
                        // Get current time for the timer
                        let now = js_sys::Date::now();
                        self.incoming_trades.update(|trades| {
                            trades.push(PendingTradeOffer {
                                offer_id,
                                proposer_id,
                                target_player_id,
                                offering,
                                requesting,
                                received_at: now,
                            });
                        });
                    }

                    // Show message
                    let proposer_name = self.players.get_untracked()
                        .iter()
                        .find(|p| p.player_id == proposer_id)
                        .map(|p| p.name.clone())
                        .unwrap_or_else(|| format!("Player {}", proposer_id));
                    self.messages.update(|m| m.push(format!("{} proposed a trade", proposer_name)));
                }
                ServerMessage::TradeCompleted { offer_id, proposer_id, accepter_id, proposer_gave: _, accepter_gave: _ } => {
                    logging::log!("Trade completed: {} between {} and {}", offer_id, proposer_id, accepter_id);

                    // Remove from incoming trades
                    self.incoming_trades.update(|trades| {
                        trades.retain(|t| t.offer_id != offer_id);
                    });

                    // Clear my pending trade if it was mine
                    if self.my_pending_trade.get_untracked() == Some(offer_id) {
                        self.my_pending_trade.set(None);
                    }

                    // Show message
                    let proposer_name = self.players.get_untracked()
                        .iter()
                        .find(|p| p.player_id == proposer_id)
                        .map(|p| p.name.clone())
                        .unwrap_or_else(|| format!("Player {}", proposer_id));

                    let accepter_name = self.players.get_untracked()
                        .iter()
                        .find(|p| p.player_id == accepter_id)
                        .map(|p| p.name.clone())
                        .unwrap_or_else(|| format!("Player {}", accepter_id));

                    self.messages.update(|m| m.push(format!("Trade completed between {} and {}", proposer_name, accepter_name)));
                }
                ServerMessage::TradeCancelled { offer_id } => {
                    logging::log!("Trade cancelled: {}", offer_id);

                    // Remove from incoming trades
                    self.incoming_trades.update(|trades| {
                        trades.retain(|t| t.offer_id != offer_id);
                    });

                    // Clear my pending trade if it was mine
                    if self.my_pending_trade.get_untracked() == Some(offer_id) {
                        self.my_pending_trade.set(None);
                    }
                }
                ServerMessage::TradeDeclined { offer_id, decliner_id } => {
                    logging::log!("Trade declined: {} by player {}", offer_id, decliner_id);

                    let decliner_name = self.players.get_untracked()
                        .iter()
                        .find(|p| p.player_id == decliner_id)
                        .map(|p| p.name.clone())
                        .unwrap_or_else(|| format!("Player {}", decliner_id));

                    // If I'm the one who declined, remove from my incoming trades
                    if Some(decliner_id) == self.player_id.get_untracked() {
                        self.incoming_trades.update(|trades| {
                            trades.retain(|t| t.offer_id != offer_id);
                        });
                    }

                    self.messages.update(|m| m.push(format!("{} declined the trade", decliner_name)));
                }
                ServerMessage::FullStateSync {
                    player_id,
                    players,
                    board,
                    game_phase,
                    current_turn_player_id,
                    robber_pos,
                    last_dice_roll,
                } => {
                    logging::log!("Full state sync received for player {}", player_id);
                    self.player_id.set(Some(player_id));
                    self.players.set(players.clone());
                    self.hexes.set(board.hexes);
                    self.settlements.set(board.settlements);
                    self.cities.set(board.cities);
                    self.roads.set(board.roads);
                    self.robber_pos.set(Some(robber_pos));
                    self.board_ports.set(board.ports);
                    self.game_phase.set(game_phase.clone());
                    self.current_turn_player.set(current_turn_player_id);
                    if let Some(my_player) = players.iter().find(|p| p.player_id == player_id) {
                        self.my_dev_cards.set(my_player.dev_cards.clone());
                        self.my_ports.set(my_player.ports.clone());
                    }
                    self.messages.update(|m| m.push("Game state synchronized.".to_string()));
                    self.my_resources.set(players.iter()
                        .find(|p| p.player_id == player_id)
                        .map(|p| p.resources.clone())
                        .unwrap_or_default());
                    self.last_dice_roll.set(last_dice_roll);
                }
                ServerMessage::PlayerWon {
                    player_id,
                    secret_victory_points
                } => {
                    logging::log!("Player {} has won the game!", player_id);

                    let player_name = self.players.get_untracked()
                        .iter()
                        .find(|p| p.player_id == player_id)
                        .map(|p| p.name.clone())
                        .unwrap_or_else(|| format!("Player {}", player_id));
                    self.winner_player_id.set(Some(player_id));

                    self.messages.update(|m| m.push(format!(
                        "{} has won the game with {} secret victory points!",
                        player_name,
                        secret_victory_points
                    )));
                }
                ServerMessage::PlayerSecretVictoryPointsUpdated { secret_victory_points} =>
                    {
                        self.secret_victory_points.set(secret_victory_points);
                    }
                ServerMessage::Left {player_id, game_id} => {
                    logging::log!("Player {} has left the game {}", player_id, game_id);
                    self.is_in_game.set(false);
                    self.game_id.set(None);
                    let player_name = self.players.get_untracked()
                        .iter()
                        .find(|p| p.player_id == player_id)
                        .map(|p| p.name.clone())
                        .unwrap_or_else(|| format!("Player {}", player_id));
                    self.players.update(|players| {
                        players.retain(|p| p.player_id != player_id);
                    });
                    self.messages.update(|m| m.push(format!(
                        "{} has left the game.",
                        player_name
                    )));
                }
            }
        }
    }

    /// Calculate the best trading ratio for a given resource based on player's ports
    pub fn get_best_ratio(&self, resource: shared::ResourceType) -> u8 {
        let ports = self.my_ports.get();
        let mut ratio = 4u8; // Default 4:1

        for port in &ports {
            match port {
                shared::PortType::ThreeToOne => {
                    ratio = ratio.min(3);
                }
                shared::PortType::TwoToOne(res) => {
                    // Check if this 2:1 port matches the resource
                    let matches = match (res, &resource) {
                        (shared::ResourceType::Brick, shared::ResourceType::Brick) => true,
                        (shared::ResourceType::Wood, shared::ResourceType::Wood) => true,
                        (shared::ResourceType::Wool, shared::ResourceType::Wool) |
                        (shared::ResourceType::Wool, shared::ResourceType::Sheep) |
                        (shared::ResourceType::Sheep, shared::ResourceType::Wool) |
                        (shared::ResourceType::Sheep, shared::ResourceType::Sheep) => true,
                        (shared::ResourceType::Wheat, shared::ResourceType::Wheat) => true,
                        (shared::ResourceType::Ore, shared::ResourceType::Ore) => true,
                        _ => false,
                    };
                    if matches {
                        return 2; // 2:1 is the best possible
                    }
                }
            }
        }

        ratio
    }
}

pub fn provide_game_state() {
    let player_id_to_use = (|| {
        let window = web_sys::window()?;
        let storage = window.local_storage().ok()??;
        if let Ok(Some(id_str)) = storage.get_item("catan_player_id") {
            if let Ok(parsed) = Uuid::parse_str(&id_str) {
                return Some(parsed);
            }
        }
        let new_id = Uuid::new_v4();
        let _ = storage.set_item("catan_player_id", &new_id.to_string());
        Some(new_id)
    })().unwrap_or_else(Uuid::new_v4);
    let state = GameState {
        // Lobby state
        messages: create_rw_signal(Vec::new()),
        is_in_game: create_rw_signal(false),
        lobby_games: create_rw_signal(Vec::new()),
        ws_sender: create_rw_signal(None),
        connection_error: create_rw_signal(None),
        is_connecting: create_rw_signal(true),

        // Game state
        game_id: create_rw_signal(None),
        player_id: create_rw_signal(Some(player_id_to_use)),
        players: create_rw_signal(Vec::new()),
        current_turn_player: create_rw_signal(Uuid::nil()),
        my_resources: create_rw_signal(Resources::default()),
        game_phase: create_rw_signal(GamePhase::WaitingForPlayers),

        // Board state
        hexes: create_rw_signal(Vec::new()),
        settlements: create_rw_signal(Vec::new()),
        cities: create_rw_signal(Vec::new()),
        roads: create_rw_signal(Vec::new()),
        robber_pos: create_rw_signal(None),
        board_ports: create_rw_signal(Vec::new()),

        // UI state
        last_dice_roll: create_rw_signal(None),
        build_mode: create_rw_signal(BuildMode::None),
        my_dev_cards: create_rw_signal(Vec::new()),

        // Robber state
        must_discard_count: create_rw_signal(None),
        must_move_robber: create_rw_signal(false),
        must_steal_from_players: create_rw_signal(Vec::new()),
        waiting_for_discards: create_rw_signal(false),

        // Road Builder state
        free_roads_remaining: create_rw_signal(0),

        // Year of Plenty state
        year_of_plenty_pending: create_rw_signal(false),

        // Monopoly state
        monopoly_pending: create_rw_signal(false),

        // Port trading state
        my_ports: create_rw_signal(Vec::new()),

        // Player-to-player trading state
        incoming_trades: create_rw_signal(Vec::new()),
        my_pending_trade: create_rw_signal(None),
        // Winner
        winner_player_id: create_rw_signal(None),
        secret_victory_points: create_rw_signal(0),
    };

    let state_clone = state;
    spawn_local(async move {
        let ws_url = format!("ws://127.0.0.1:8080/ws?id={}", player_id_to_use);
        logging::log!("Connecting to WebSocket at {}", ws_url);
        match WebSocket::open(&ws_url) {
            Ok(ws) => {
                logging::log!("WebSocket Connected!");
                let (mut write, mut read) = ws.split();
                let (tx, mut rx) = futures::channel::mpsc::unbounded::<Message>();

                state_clone.ws_sender.set(Some(tx));
                state_clone.is_connecting.set(false);
                state_clone.connection_error.set(None);

                state_clone.send(ClientRequest::GetLobbyList);

                spawn_local(async move {
                    while let Some(msg) = rx.next().await {
                        let _ = write.send(msg).await;
                    }
                });

                spawn_local(async move {
                    while let Some(msg) = read.next().await {
                        if let Ok(Message::Text(text)) = msg {
                            state_clone.process_message(text);
                        }
                    }
                    state_clone.connection_error.set(Some("Connection to server closed.".into()));
                    state_clone.ws_sender.set(None);
                });
            }
            Err(e) => {
                state_clone.is_connecting.set(false);
                state_clone.connection_error.set(Some(format!("Failed to connect: {:?}", e)));
            }
        }
    });

    provide_context(state);
}