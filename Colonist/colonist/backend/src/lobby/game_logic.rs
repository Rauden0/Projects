use log::info;
use shared::GamePhase;
use crate::lobby::Lobby;

impl Lobby
{
    pub(crate) fn advance_initial_placement(&mut self, gid: &str) -> bool {
        let game = match self.games.get_mut(gid) {
            Some(g) => g,
            None => return false,
        };

        let max_settlements = game.max_players * 2;

        if game.initial_settlements_placed >= max_settlements {
            info!("Initial placement complete, transitioning to regular play");
            game.phase = GamePhase::RegularPlay;
            game.turn_manager.reset_order();
            return true;  // Signal that we transitioned to regular play
        }

        let settlements_in_round1 = game.max_players;
        if game.initial_settlements_placed <= settlements_in_round1 {
            if game.initial_settlements_placed == settlements_in_round1 {
                info!("Transitioning to initial placement round 2 - player {} goes again", game.turn_manager.current_player_index);
                game.phase = GamePhase::InitialPlacementRound2;
            } else {
                let next_index = game.turn_manager.current_player_index + 1;
                if let Some(&next_player_id) = game.player_ids.get(next_index) {
                    game.turn_manager.current_player_index = next_index;
                    game.turn_manager.current_player_id = next_player_id;
                }
            }
        } else {
            let next_index = if game.turn_manager.current_player_index == 0 {
                game.max_players - 1
            } else {
                game.turn_manager.current_player_index - 1
            };
            if let Some(&next_player_id) = game.player_ids.get(next_index) {
                game.turn_manager.current_player_index = next_index;
                game.turn_manager.current_player_id = next_player_id;
            }
        }

        info!("Initial placement: Now player {} (slot {})'s turn", game.turn_manager.current_player_id, game.turn_manager.current_player_index);
        false  // Did not transition to regular play
    }
}