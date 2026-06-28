use std::collections::HashMap;
use serde::{Deserialize, Serialize};
use uuid::Uuid;
use crate::game::entities::player::Player;

pub trait BonusCard {

    fn points(&self) -> u8;

    fn holder(&self) -> Option<Uuid>;
    fn set_holder(&mut self, holder: Option<Uuid>);

    fn minimum_number(&self) -> usize;

    fn player_number(&self, player: &Player) -> usize;

    fn recalculate(&mut self, players: &mut HashMap<Uuid, Player>) {
        let mut best_player: Option<Uuid> = None;
        let mut best_value: usize = 0;
        let mut tie = false;

        for (pid, player) in players.iter() {
            let value = self.player_number(player);

            if value > best_value {
                best_value = value;
                best_player = Some(*pid);
                tie = false;
            } else if value == best_value && value != 0 {
                tie = true;
            }
        }

        if tie {
            return;
        }

        if best_value < self.minimum_number() {
            if let Some(old_holder) = self.holder() {
                if let Some(p) = players.get_mut(&old_holder) {
                    for _ in 0..self.points() {
                        p.remove_victory_point();
                    }
                }
            }
            self.set_holder(None);
            return;
        }

        let best_player = best_player.unwrap();

        if self.holder() == Some(best_player) {
            return;
        }

        if let Some(old_holder) = self.holder() {
            if let Some(p) = players.get_mut(&old_holder) {
                for _ in 0..self.points() {
                    p.remove_victory_point();
                }
            }
        }

        if let Some(p) = players.get_mut(&best_player) {
            for _ in 0..self.points() {
                p.add_victory_point();
            }
        }

        self.set_holder(Some(best_player));
    }
}

#[derive(Serialize, Deserialize, Clone)]
pub struct LongestRoad {
    holder: Option<Uuid>,
}

impl LongestRoad {
    pub fn new() -> Self {
        Self {
            holder: None,
        }
    }
}

impl BonusCard for LongestRoad {
    fn points(&self) -> u8 { 2 }
    fn holder(&self) -> Option<Uuid> { self.holder }
    fn set_holder(&mut self, h: Option<Uuid>) { self.holder = h }
    fn minimum_number(&self) -> usize { 5 }
    fn player_number(&self, player: &Player) -> usize {
        player.longest_road
    }
}

#[derive(Serialize, Deserialize, Clone)]
pub struct BiggestArmy {
    holder: Option<Uuid>,
}

impl BiggestArmy {
    pub fn new() -> Self {
        Self {
            holder: None,
        }
    }
}

impl BonusCard for BiggestArmy {
    fn points(&self) -> u8 { 2 }
    fn holder(&self) -> Option<Uuid> { self.holder }
    fn set_holder(&mut self, h: Option<Uuid>) { self.holder = h }
    fn minimum_number(&self) -> usize { 3 }
    fn player_number(&self, player: &Player) -> usize {
        player.knight_played
    }
}
