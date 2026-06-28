use std::collections::{HashMap, HashSet};
use crate::errors::GameError;
use crate::game::entities::board::{Board, Coordinates, PortType};
use crate::game::entities::development_card::DevelopmentCard::Knight;
use crate::game::entities::development_card::{DevCardState, DevelopmentCard};
use crate::game::entities::player::Player;
use crate::game::entities::resources::ResourceSet;
use crate::game::entities::robber::Robber;
use serde::{Deserialize, Serialize};
use serde_with::serde_as;
use uuid::Uuid;
use shared::ResourceType;
const RESOURCES: u32 = 50;
#[serde_as]
#[derive(Serialize, Deserialize, Debug, Clone)]
pub struct Bank {
    #[serde_as(as = "Vec<(_, _)>")]
    pub players: HashMap<Uuid, Player>,
    pub game_resources: ResourceSet,
    pub dev_cards: Vec<DevelopmentCard>,
}

impl Bank {
    pub fn new() -> Self {
        let mut resources = ResourceSet::new();
        resources.add(ResourceType::Brick, RESOURCES);
        resources.add(ResourceType::Ore, RESOURCES);
        resources.add(ResourceType::Sheep, RESOURCES);
        resources.add(ResourceType::Wheat, RESOURCES);
        resources.add(ResourceType::Wood, RESOURCES);

        let mut dev_cards: Vec<DevelopmentCard> = Vec::new();
        for _ in 0..14 {
            dev_cards.push(Knight(DevCardState::new()));
        }
        for _ in 0..5 {
            dev_cards.push(DevelopmentCard::VictoryPoint(DevCardState::new()));
        }
        for _ in 0..2 {
            dev_cards.push(DevelopmentCard::RoadBuilder(DevCardState::new()));
        }
        for _ in 0..2 {
            dev_cards.push(DevelopmentCard::Monopoly(DevCardState::new()));
        }
        for _ in 0..2 {
            dev_cards.push(DevelopmentCard::YearOfPlenty(DevCardState::new()));
        }

        use rand::seq::SliceRandom;
        let mut rng = rand::thread_rng();
        dev_cards.shuffle(&mut rng);

        Self {
            players: HashMap::new(),
            game_resources: resources,
            dev_cards,
        }
    }

    pub fn add_player(&mut self, player: Player) {
        self.players.insert(player.id, player);
    }


    pub fn give_resources_for_roll(&mut self, board: &Board, dice_number: u8, robber: &Robber) -> Vec<(Uuid, ResourceType, u32)> {
        let mut pending: Vec<(Uuid, ResourceType, u32)> = Vec::new();
        let mut totals: HashMap<ResourceType, u32> = HashMap::new();

        for hex in board.hexes.values() {
            if hex.number != dice_number
                || hex.resource == ResourceType::Desert
                || robber.pos == hex.coord
            {
                continue;
            }

            for &vcoord in &hex.adjacent_vertices {
                let Some(vertex) = board.vertices.get(&vcoord) else {
                    continue;
                };
                let Some(building) = &vertex.building else {
                    continue;
                };
                let Some(player_id) = vertex.owner else {
                    continue;
                };

                let amount = building.production();

                pending.push((player_id, hex.resource, amount));
                *totals.entry(hex.resource).or_insert(0) += amount;
            }
        }

        let mut can_distribute: HashSet<ResourceType> = HashSet::new();
        for (res, &needed) in &totals {
            let available = self.game_resources.amount_of(*res);
            if available < needed {
                println!(
                    "Bank does not have enough {:?}: needed {}, available {}",
                    res, needed, available
                );
            } else {
                can_distribute.insert(*res);
            }
        }

        let mut distributed: Vec<(Uuid, ResourceType, u32)> = Vec::new();
        for (player_id, res, amount) in pending {
            if !can_distribute.contains(&res) {
                continue;
            }

            self.game_resources.take(res, amount);
            if let Some(player) = self.players.get_mut(&player_id) {
                player.add_resource(res, amount);
                distributed.push((player_id, res, amount));
            }
        }
        distributed
    }

    /// Give resources to a player for their initial settlement placement (only during round 2)
    pub fn give_initial_settlement_resources(
        &mut self,
        board: &Board,
        player_id: Uuid,
        settlement_pos: Coordinates,
    ) {
        use log::info;

        info!(
            "Player {} placing second settlement at {:?}, distributing initial resources",
            player_id, settlement_pos
        );

        let mut resources_given = 0;
        // Find all hexes that have this vertex as one of their adjacent vertices
        for (_, hex) in board.hexes.iter() {
            // Check if this hex has the settlement vertex as one of its adjacent vertices
            if !hex.adjacent_vertices.contains(&settlement_pos) {
                continue;
            }

            // Skip desert tiles
            if hex.resource == ResourceType::Desert {
                info!("  - Skipping desert hex at {:?}", hex.coord);
                continue;
            }

            // Check if bank has the resource
            let available = self.game_resources.amount_of(hex.resource);
            if available > 0 {
                self.game_resources.take(hex.resource, 1);
                if let Some(player) = self.players.get_mut(&player_id) {
                    player.add_resource(hex.resource, 1);
                    resources_given += 1;
                    info!(
                        "  - Gave 1 {:?} from hex {:?} (bank had {})",
                        hex.resource, hex.coord, available
                    );
                }
            } else {
                info!(
                    "  - FAILED: Bank out of {:?} for hex {:?}",
                    hex.resource, hex.coord
                );
            }
        }

        info!(
            "Player {} received {} resources total from second settlement",
            player_id, resources_given
        );
    }

    pub fn trade_with_bank(
        &mut self,
        player_id: Uuid,
        gives: ResourceSet,
        takes: ResourceSet,
    ) -> Result<(), GameError> {
        {
            let player = self
                .players
                .get(&player_id)
                .ok_or(GameError::PlayerNotFound)?;

            if !player.can_pay(&gives) {
                return Err(GameError::NotEnoughResources);
            }

            if !self.game_resources.can_pay(&takes) {
                return Err(GameError::NotEnoughResources);
            }

            let mut produced_units: u32 = 0;
            for (res, amount) in &gives.amounts {
                let ratio = self.best_ratio(player, *res);

                if amount % ratio != 0 {
                    return Err(GameError::WrongResourceRatio);
                }

                produced_units += amount / ratio;
            }

            let takes_total: u32 = takes.amounts.values().sum();

            if produced_units != takes_total {
                return Err(GameError::WrongResourceRatio);
            }
        }

        self.collect_from_player(player_id, gives)?;

        let player = self
            .players
            .get_mut(&player_id)
            .ok_or(GameError::PlayerNotFound)?;
        player.resources.add_set(&takes);

        self.game_resources.take_set(&takes);

        Ok(())
    }

    pub fn best_ratio(&self, player: &Player, res: ResourceType) -> u32 {
        let mut ratio = 4;

        for port in &player.ports {
            match port {
                PortType::ThreeToOne => ratio = ratio.min(3),
                PortType::TwoToOne(r) if *r == res => return 2,
                _ => {}
            }
        }

        ratio
    }

    pub fn collect_from_player(
        &mut self,
        player_id: Uuid,
        cost: ResourceSet,
    ) -> Result<(), GameError> {
        let player = self
            .players
            .get_mut(&player_id)
            .ok_or(GameError::PlayerNotFound)?;
        if !player.can_pay(&cost) {
            return Err(GameError::NotEnoughResources);
        }

        player.pay(&cost);
        self.game_resources.add_set(&cost);

        Ok(())
    }

    pub fn collect_from_player_to_player(
        &mut self,
        from_id: Uuid,
        to_id: Uuid,
        cost: &ResourceSet,
    ) -> Result<(), GameError> {
        if !self.players.contains_key(&from_id) {
            return Err(GameError::PlayerNotFound);
        }
        if !self.players.contains_key(&to_id) {
            return Err(GameError::PlayerNotFound);
        }

        let mut from_player = self.players.remove(&from_id).unwrap();
        if !from_player.can_pay(cost) {
            self.players.insert(from_id, from_player);
            return Err(GameError::NotEnoughResources);
        }

        from_player.pay(cost);

        let to_player = self.players.get_mut(&to_id).unwrap();
        to_player.resources.add_set(cost);

        self.players.insert(from_id, from_player);

        Ok(())
    }

    pub fn collect_resource_from_all_to_player(&mut self, to_id: Uuid, resource: ResourceType) -> Result<u32, GameError> {
        let transfers: Vec<(Uuid, u32)> = self
            .players
            .iter()
            .filter(|(player_id, _)| *player_id != &to_id)
            .map(|(&player_id, player)| (player_id, player.resources.amount_of(resource)))
            .filter(|(_, amt)| *amt > 0)
            .collect();

        let mut total_stolen = 0u32;
        for (from_id, amt) in transfers {
            let mut cost = ResourceSet::new();
            cost.add(resource, amt);
            self.collect_from_player_to_player(from_id, to_id, &cost)?;
            total_stolen += amt;
        }

        Ok(total_stolen)
    }
}
