use crate::game::entities::bonus_points::BonusCard;
use crate::game::entities::pending_trade::PendingTrade;
use crate::game::entities::turn_manager::TurnManager;
use serde::{Deserialize, Serialize};
use shared::GamePhase;
use std::collections::{HashMap, HashSet};
use uuid::Uuid;

#[derive(Serialize, Deserialize, Clone)]
pub struct GameInstance {
    pub id: String,
    pub player_ids: Vec<Uuid>,  // player IDs in join order
    pub player_id_to_slot: HashMap<Uuid, usize>,  // Maps connection ID to game slot (0-3)
    pub turn_manager: TurnManager,
    pub max_players: usize,
    pub phase: GamePhase,
    pub initial_settlements_placed: usize,  // Track how many initial settlements have been placed
    pub pending_discards: HashSet<Uuid>,  // Players who still need to discard
    pub seven_roller: Option<Uuid>,  // Player who rolled 7 and needs to move robber
    pub knight_mover: Option<Uuid>,  // Player who played a knight and needs to move robber
    pub free_roads_remaining: u8,  // Free roads from Road Builder card
    pub year_of_plenty_pending: Option<Uuid>,  // Player who played Year of Plenty and needs to choose resources
    pub monopoly_pending: Option<Uuid>,
    #[serde(skip)]
    pub pending_trades: HashMap<u64, PendingTrade>,  // Active trade offers
    #[serde(skip)]
    pub next_trade_id: u64,  // Counter for trade IDs
}
impl GameInstance {
    pub fn get_all_players_info(&self) -> Vec<shared::PlayerInfo> {
        self.player_ids.iter()
            .filter_map(|&pid| self.turn_manager.get_player_info(pid).map(|p| (pid, p)))
            .map(|(pid, p)| {
                let has_longest_road = self.turn_manager.road_bonus.holder() == Some(pid);
                let has_largest_army = self.turn_manager.army_bonus.holder() == Some(pid);

                shared::PlayerInfo {
                    player_id: pid,
                    name: p.name.clone(),
                    color: p.colour.to_string(),
                    victory_points: p.get_victory_points(),
                    dev_cards: p.dev_cards.iter().map(|c| c.get_type()).collect(),
                    ports: p.ports.iter().map(|port| port.into()).collect(),
                    resources: (&p.resources).into(),
                    knights_played: p.knight_played,
                    roads_count: p.longest_road,
                    has_longest_road,
                    has_largest_army,
                }
            })
            .collect()
    }
    pub fn can_build_settlement_in_phase(&self, pid: Uuid) -> Result<(), String> {
        let building_count = self.turn_manager.board.get_number_of_buildings(pid);

        match self.phase {
            GamePhase::InitialPlacementRound1 => {
                // Round 1: Exactly 0 buildings allowed before placement
                if building_count >= 1 {
                    return Err("You must place exactly one settlement in the first round.".into());
                }
            }
            GamePhase::InitialPlacementRound2 => {
                // Round 2: Exactly 1 building allowed before placement
                if building_count >= 2 {
                    return Err("You have already placed your second initial settlement.".into());
                }
                if building_count < 1 {
                    return Err("Invalid state: Missing first settlement.".into());
                }
            }
            _ => {}
        }
        Ok(())
    }
    pub fn can_build_road_in_phase(&self, pid: Uuid) -> Result<(), String> {
        let road_count = self.turn_manager.board.get_number_of_roads(pid);
        let settlement_count = self.turn_manager.board.get_number_of_buildings(pid);
        match self.phase {
            GamePhase::InitialPlacementRound1 | GamePhase::InitialPlacementRound2 => {
                if road_count >= settlement_count {
                    return Err("You must place a settlement before placing a road.".into());
                }
            }
            _ => {}
        }
        Ok(())
    }
}
