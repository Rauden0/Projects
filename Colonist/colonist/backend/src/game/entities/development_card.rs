use crate::game::entities::bonus_points::BonusCard;
use crate::game::entities::turn_manager::TurnManager;
use shared::{DevCardTarget, DevCardType};
use std::fmt::Debug;
use serde::{Serialize, Deserialize};

#[derive(Debug, Clone, Serialize, Deserialize)]
pub struct DevCardState {
    pub bought_this_turn: bool,
    pub played: bool,
}

impl DevCardState {
    pub fn new() -> Self {
        Self {
            bought_this_turn: true,
            played: false,
        }
    }

    pub fn next_turn(&mut self) {
        self.bought_this_turn = false;
    }

    pub fn can_play(&self) -> bool {
        !self.bought_this_turn && !self.played
    }
}

#[derive(Debug, Clone, Serialize, Deserialize)]
pub enum DevelopmentCard {
    Knight(DevCardState),
    VictoryPoint(DevCardState),
    RoadBuilder(DevCardState),
    YearOfPlenty(DevCardState),
    Monopoly(DevCardState),
}

impl DevelopmentCard {
    pub fn new(card_type: DevCardType) -> Self {
        let state = DevCardState::new();
        match card_type {
            DevCardType::Knight => Self::Knight(state),
            DevCardType::VictoryPoint => Self::VictoryPoint(state),
            DevCardType::RoadBuilding => Self::RoadBuilder(state),
            DevCardType::YearOfPlenty => Self::YearOfPlenty(state),
            DevCardType::Monopoly => Self::Monopoly(state),
        }
    }

    pub fn glyph(&self) -> char {
        match self {
            Self::Knight(_) => 'K',
            Self::VictoryPoint(_) => 'V',
            Self::RoadBuilder(_) => 'R',
            Self::YearOfPlenty(_) => 'Y',
            Self::Monopoly(_) => 'M',
        }
    }

    pub fn can_play(&self) -> bool {
        match self {
            // Victory Point cards are never playable - they automatically count as VP
            Self::VictoryPoint(_) => false,
            Self::Knight(s) | Self::RoadBuilder(s) | Self::YearOfPlenty(s) | Self::Monopoly(s) => s.can_play(),
        }
    }

    pub fn next_turn(&mut self) {
        match self {
            Self::Knight(s) | Self::RoadBuilder(s) | Self::YearOfPlenty(s) | Self::Monopoly(s) => s.next_turn(),
            Self::VictoryPoint(_) => {},
        }
    }

    pub fn play(&mut self, game: &mut TurnManager, _target: &Option<DevCardTarget>) {
        if !self.can_play() {
            return;
        }

        match self {
            Self::Knight(s) => {
                let pid = game.current_player_id();
                if let Some(player) = game.bank.players.get_mut(&pid) {
                    player.knight_played += 1;
                }
                game.army_bonus.recalculate(&mut game.bank.players);
                s.played = true;
            }
            Self::VictoryPoint(_) => {
                // Victory Point cards cannot be played - they grant VP automatically when bought
                unreachable!("Victory Point cards should never be played");
            }
            Self::RoadBuilder(s) => {
                s.played = true;
            }
            Self::YearOfPlenty(s) => {
                // Resources are given after the player chooses via YearOfPlentyChoice request
                // Just mark the card as played here
                s.played = true;
            }
            Self::Monopoly(s) => {
                // Resource collection happens after the player chooses via MonopolyChoice request
                // Just mark the card as played here
                s.played = true;
            }
        }
    }

    pub fn get_type(&self) -> DevCardType {
        match self {
            Self::Knight(_) => DevCardType::Knight,
            Self::VictoryPoint(_) => DevCardType::VictoryPoint,
            Self::RoadBuilder(_) => DevCardType::RoadBuilding,
            Self::YearOfPlenty(_) => DevCardType::YearOfPlenty,
            Self::Monopoly(_) => DevCardType::Monopoly,
        }
    }
}