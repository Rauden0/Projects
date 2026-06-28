use uuid::Uuid;
use shared::{ServerMessage, GamePhase};
use crate::lobby::GameInstance;

impl GameInstance
{
    pub fn handle_roll_dice(&mut self, pid: Uuid) -> Result<ServerMessage, String> {
        let tm = &mut self.turn_manager;

        if pid != tm.current_player_id { return Err("Wait for your turn!".to_string()); }

        if matches!(self.phase, GamePhase::InitialPlacementRound1 | GamePhase::InitialPlacementRound2) {
            return Err("Cannot roll dice during initial placement!".to_string());
        }

        tm.next_turn()
            .map(|(roll, _)| {
                let d1 = roll / 2;
                let d2 = roll - d1;
                let mut discards_count = 0;

                if roll == 7 {
                    self.seven_roller = Some(pid);
                    self.pending_discards.clear();
                    for (&p_id, player) in &mut tm.bank.players {
                        let total = player.resources.get_cards_total();
                        if total > 7 {
                            self.pending_discards.insert(p_id);
                            discards_count += 1;
                        }
                    }
                }

                ServerMessage::DiceRolled {
                    player_id: pid,
                    dice_1: d1,
                    dice_2: d2,
                    discards_pending: discards_count,
                }
            })
            .map_err(|e| format!("{:?}", e))
    }
    pub fn handle_end_turn(&mut self, pid: Uuid) -> Result<ServerMessage, String> {
        let tm = &mut self.turn_manager;

        if pid != tm.current_player_id { return Err("Wait for your turn!".to_string()); }
        if self.phase != GamePhase::RegularPlay { return Err("Cannot end turn during initial placement!".to_string()); }
        if self.seven_roller.is_some() {
            return Err("Cannot end turn until you move the robber".to_string());
        }
        tm.end_turn();
        Ok(ServerMessage::NextTurn { player_id: tm.current_player_id  })
    }
}


