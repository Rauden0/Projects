use serde::{Deserialize, Serialize};
use uuid::Uuid;
use shared::{PlayerInfo, ResourceType, Resources};
use crate::errors::GameError;
use crate::game::entities::board::PortType;
use crate::game::entities::development_card::DevelopmentCard;
use crate::game::entities::resources::{ResourceSet};

#[derive(Debug, Clone, Serialize, Deserialize)]
pub struct Player {
    pub id: Uuid,
    pub name: String,
    pub resources: ResourceSet,
    
    pub colour: char,

    pub(crate) settlements_left: u8,
    cities_left: u8,
    roads_left: u8,

    pub dev_cards: Vec<DevelopmentCard>,
    pub dev_card_played_this_turn: bool,

    pub knight_played: usize,
    pub longest_road: usize,

    victory_points: u8,
    // Used for victory point cards from development cards
    secret_victory_points: u8,

    pub ports: Vec<PortType>,
}

impl Player {
    pub fn new(id: Uuid, name: &str, colour: char) -> Self {
        Self {
            id,
            name: name.to_string(),
            resources: ResourceSet::new(),
            colour,
            settlements_left: 5,
            cities_left: 4,
            roads_left: 15,
            dev_cards: Vec::new(),
            knight_played: 0,
            longest_road: 0,
            victory_points: 0,
            ports: Vec::new(),
            dev_card_played_this_turn: false,
            secret_victory_points: 0,
        }
    }

    pub fn add_victory_point(&mut self) {
        self.victory_points += 1;
    }
    pub fn add_secret_victory_point(&mut self) {
        self.secret_victory_points += 1;
    }
    pub fn get_total_victory_points(&self) -> u8 {
        self.victory_points + self.secret_victory_points
    }
    pub fn get_secret_victory_points(&self) -> u8 {
        self.secret_victory_points
    }


    pub fn remove_victory_point(&mut self) {
        self.victory_points -= 1;
    }

    pub fn get_victory_points(&self) -> u8 {
        self.victory_points
    }

    pub fn can_pay(&self, cost: &ResourceSet) -> bool {
        self.resources.can_pay(cost)
    }

    pub fn pay(&mut self, cost: &ResourceSet) -> bool {
        if !self.can_pay(cost) {
            return false;
        }
        self.resources.take_set(cost);
        true
    }

    pub fn add_resource(&mut self, res: ResourceType, amount: u32) {
        self.resources.add(res, amount);
    }

    pub fn has_settlement(&self) -> bool {
        self.settlements_left > 0
    }

    pub fn has_city(&self) -> bool {
        self.cities_left > 0
    }

    pub fn has_road(&self) -> bool {
        self.roads_left > 0
    }


    pub fn use_settlement(&mut self) -> Result<(), GameError> {
        if !self.has_settlement() {
            return Err(GameError::NotEnoughBuildingsOfThisType);
        }
        self.settlements_left -= 1;
        self.add_victory_point();
        Ok(())
    }

    pub fn use_city(&mut self) -> Result<(), GameError> {
        if !self.has_city() {
            return Err(GameError::NotEnoughBuildingsOfThisType);
        }
        self.cities_left -= 1;
        self.settlements_left += 1;
        self.add_victory_point();
        Ok(())
    }

    pub fn use_road(&mut self) -> Result<(), GameError> {
        if !self.has_road() {
            return Err(GameError::NotEnoughBuildingsOfThisType);
        }
        self.roads_left -= 1;
        Ok(())
    }

}
impl From<&Player> for PlayerInfo {
    fn from(p: &Player) -> Self {
        PlayerInfo {
            player_id: p.id,
            name: p.name.clone(),
            color: p.colour.to_string(),
            victory_points: p.get_victory_points(),
            dev_cards: p.dev_cards.iter().map(|c| c.get_type()).collect(),
            ports: p.ports.iter().map(|port| port.into()).collect(),
            resources: Resources::from(&p.resources),
            knights_played: p.knight_played,
            roads_count: p.longest_road,
            has_longest_road: false,  // Cannot determine without TurnManager context
            has_largest_army: false,  // Cannot determine without TurnManager context
        }
    }
}
