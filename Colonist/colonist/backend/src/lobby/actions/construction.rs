use crate::lobby::GameInstance;
use shared::{GamePhase, ServerMessage};
use uuid::Uuid;

impl GameInstance
{
    pub fn handle_build_settlement(&mut self, pid: Uuid, x: i32, y: i32) -> Result<ServerMessage, String> {
        let is_initial_phase = matches!(self.phase, GamePhase::InitialPlacementRound1 | GamePhase::InitialPlacementRound2);

        if pid != self.turn_manager.current_player_id {
            return Err("Wait for your turn!".to_string());
        }
        self.can_build_settlement_in_phase(pid)?;
        self.turn_manager.build_settlement((x, y), is_initial_phase)
            .map(|_| {
                if is_initial_phase {
                    self.initial_settlements_placed += 1;
                    // Give initial resources for second settlement (round 2 only)
                    if self.phase == GamePhase::InitialPlacementRound2 {
                        self.turn_manager.bank.give_initial_settlement_resources(&self.turn_manager.board, pid, (x, y));
                    }
                }

                ServerMessage::Built {
                    player_id: pid,
                    structure_type: "SETTLEMENT".into(),
                    coords: vec![x, y],
                }
            })
            .map_err(|e| format!("{:?}", e))
    }




    pub fn handle_build_city(&mut self,pid: Uuid, x: i32, y: i32) -> Result<ServerMessage, String> {
        let tm = &mut self.turn_manager;
        if pid != tm.current_player_id { return Err("Wait for your turn!".to_string()); }

        tm.build_city((x, y))
            .map(|_| {
                ServerMessage::Built {
                    player_id: pid,
                    structure_type: "CITY".into(),
                    coords: vec![x, y],
                }
            })
            .map_err(|e| format!("{:?}", e))
    }

    pub fn handle_build_road(&mut self, pid: Uuid, x1: i32, y1: i32) -> Result<ServerMessage, String> {
        let is_initial_phase = matches!(self.phase, GamePhase::InitialPlacementRound1 | GamePhase::InitialPlacementRound2);
        let is_free_road = self.free_roads_remaining > 0;

        if pid != self.turn_manager.current_player_id { return Err("Wait for your turn!".to_string()); }
        self.can_build_road_in_phase(pid)?;

        // Build road for free if we have free roads from Road Builder card
        self.turn_manager.build_road((x1, y1), is_initial_phase || is_free_road)
            .map(|_| {
                // Decrement free roads counter if using Road Builder
                if is_free_road {
                    self.free_roads_remaining -= 1;
                }
                ServerMessage::Built {
                    player_id: pid,
                    structure_type: "ROAD".into(),
                    coords: vec![x1, y1],
                }
            })
            .map_err(|e| format!("{:?}", e))
    }

}
