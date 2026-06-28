use crate::errors::GameError;
use crate::game::entities::bank::Bank;
use crate::game::entities::board::{Board, Coordinates};
use crate::game::entities::bonus_points::{BiggestArmy, BonusCard, LongestRoad};
// Building trait needed for cost()
use crate::game::entities::dice::Dice;
use crate::game::entities::player::Player;
use crate::game::entities::resources::ResourceSet;
use crate::game::entities::robber::Robber;
use log::info;
use serde::{Deserialize, Serialize};
use uuid::Uuid;
use shared::ResourceType::{Ore, Sheep, Wheat};
use crate::game::entities::building::EdgeBuilding::Road;
use crate::game::entities::building::VertexBuilding::{City, Settlement};
#[derive(Serialize, Deserialize, Clone)]
pub struct TurnManager {
    dice: Dice,
    pub bank: Bank,
    pub board: Board,
    pub current_player_index: usize,
    pub current_player_id: Uuid,
    pub last_roll: Option<u8>,
    pub players: Vec<Player>,
    pub game_over: bool,
    pub robber: Robber,
    player_count: usize,
    pub army_bonus: BiggestArmy,
    pub road_bonus: LongestRoad,
}

impl TurnManager {
    pub fn new(player_count: usize, first_player_id:Uuid) -> TurnManager {
        let mut bank = Bank::new();
        let first_player = Player::new(first_player_id, "Player 1", 'b');
        bank.add_player(first_player.clone());
        let board = Board::new();
        let robber = Robber::new(&board);
        let players = vec![first_player.clone()];
        info!("New game initialized for {} players", player_count);

        Self {
            dice: Dice::new(),
            bank,
            board,
            current_player_index: 0,
            current_player_id: first_player_id,
            game_over: false,
            robber,
            player_count,
            army_bonus: BiggestArmy::new(),
            road_bonus: LongestRoad::new(),
            players,
            last_roll: None,
        }
    }


    pub fn next_turn(&mut self) -> Result<(u8, Vec<(Uuid, shared::ResourceType, u32)>), GameError> {
        if self.game_over {
            return Err(GameError::InvalidAction);
        }
        if self.last_roll.is_some()
        {
            return Ok((self.last_roll.unwrap(), Vec::new()));
        }

        let roll_value = self.dice.roll();
        info!(
            "Player {} rolled: {}",
            self.current_player_index, roll_value
        );

        let distributed = if roll_value == 7 {
            info!("Robber activated (7 rolled)");
            // Logic handled by Lobby
            Vec::new()
        } else {
            self.bank
                .give_resources_for_roll(&self.board, roll_value, &self.robber)
        };
        self.last_roll = Some(roll_value);
        Ok((roll_value, distributed))
    }

    pub fn end_turn(&mut self) {
        let prev_player = self.current_player_index;
        if let Some(player) = self.bank.players.get_mut(&self.current_player_id) {
            player
                .dev_cards
                .iter_mut()
                .for_each(|card| card.next_turn());
            player.dev_card_played_this_turn = false;
        }
        self.current_player_index = (self.current_player_index + 1) % self.player_count;
        self.bank.players.iter().for_each(|(pid, _p)| info!("{}", pid));
        self.current_player_id = self.players[self.current_player_index].id;
        self.last_roll = None;
        info!(
            "Player {}'s turn started.",
            self.current_player_id
        );
        info!(
            "Turn ended for Player {}. Now on turn: Player {}",
            prev_player, self.current_player_index
        );
    }
    fn pay_resources(&mut self, pid: Uuid, cost: ResourceSet) -> Result<(), GameError> {
        {
            let player = self
                .bank
                .players
                .get(&pid)
                .ok_or(GameError::PlayerNotFound)?;
            if !player.can_pay(&cost) {
                return Err(GameError::NotEnoughResources);
            }
        }
        self.bank.collect_from_player(pid, cost)?;
        Ok(())
    }
    pub fn build_settlement(
        &mut self,
        pos: Coordinates,
        is_initial: bool,
    ) -> Result<(), GameError> {
        let pid = self.current_player_id;

        let vertex = self
            .board
            .vertices
            .get(&pos)
            .ok_or(GameError::InvalidPosition)?;
        if vertex.building.is_some() || !self.board.is_buildable_vertex(pos) {
            return Err(GameError::InvalidPosition);
        }
        if !is_initial && !self.board.is_vertex_connected_to_player(pos, pid) {
            return Err(GameError::InvalidPosition);
        }

        if !is_initial {
            self.pay_resources(pid, Settlement.cost())?;
        }

        let port_type = self.board.ports.get(&pos).map(|p| p.port_type);

        let player = self
            .bank
            .players
            .get_mut(&pid)
            .ok_or(GameError::PlayerNotFound)?;
        player.use_settlement()?;

        // Add port to player if building on a port vertex
        if let Some(pt) = port_type {
            if !player.ports.contains(&pt) {
                player.ports.push(pt);
                info!("Player {} gained access to port {:?}", pid, pt);
            }
        }

        self.board.build_vertex(pid, pos, Settlement);

        info!("Player {} built a SETTLEMENT at {:?}", pid, pos);
        Ok(())
    }

    pub fn build_city(&mut self, pos: Coordinates) -> Result<(), GameError> {
        let pid = self.current_player_id;

        let vertex = self
            .board
            .vertices
            .get(&pos)
            .ok_or(GameError::InvalidPosition)?;
        let is_owner = vertex.owner == Some(pid);
        if !is_owner {
            return Err(GameError::InvalidAction);
        }
        if vertex.building != Some(Settlement) {
            return Err(GameError::InvalidAction);
        }
        self.pay_resources(pid, City.cost())?;

        let player = self
            .bank
            .players
            .get_mut(&pid)
            .ok_or(GameError::PlayerNotFound)?;
        player.use_city()?;
        player.settlements_left += 1;

        self.board.build_vertex(pid, pos, City);

        info!("Player {} built a CITY at {:?}", pid, pos);
        Ok(())
    }

    pub fn build_road(&mut self, pos: Coordinates, is_initial: bool) -> Result<(), GameError> {
        let pid = self.current_player_id;

        let edge = self.board.edges.get(&pos).ok_or(GameError::InvalidAction)?;
        if edge.building.is_some() || !self.board.is_edge_connected_to_player(pos, pid) {
            return Err(GameError::InvalidAction);
        }

        let cost = Road.cost();

        if !is_initial {
            let player = self
                .bank
                .players
                .get(&pid)
                .ok_or(GameError::PlayerNotFound)?;
            if !player.can_pay(&cost) {
                return Err(GameError::NotEnoughResources);
            }
            self.bank.collect_from_player(pid, cost)?;
        }

        let longest_road_len = {
            let player = self
                .bank
                .players
                .get_mut(&pid)
                .ok_or(GameError::PlayerNotFound)?;
            player.use_road()?;
            self.board.build_edge(pid, pos, Road);

            self.board.calculate_longest_road(pid)
        };

        let player = self
            .bank
            .players
            .get_mut(&pid)
            .ok_or(GameError::PlayerNotFound)?;
        player.longest_road = longest_road_len;

        self.road_bonus.recalculate(&mut self.bank.players);

        info!("Player {} built a ROAD at {:?}", pid, pos);
        Ok(())
    }

    pub fn buy_dev_card(&mut self) -> Result<shared::DevCardType, GameError> {
        let pid = self.current_player_id;
        let mut cost = ResourceSet::new();
        cost.add(Sheep, 1);
        cost.add(Wheat, 1);
        cost.add(Ore, 1);

        {
            let player = self
                .bank
                .players
                .get_mut(&pid)
                .ok_or(GameError::PlayerNotFound)?;
            if !player.can_pay(&cost) {
                return Err(GameError::NotEnoughResources);
            }
        }

        self.bank.collect_from_player(pid, cost)?;

        let card = self.bank.dev_cards.pop().ok_or(GameError::InvalidAction)?;
        let card_type = card.get_type();

        let player = self
            .bank
            .players
            .get_mut(&pid)
            .ok_or(GameError::PlayerNotFound)?;

        // Victory Point cards immediately grant a secret victory point
        if card_type == shared::DevCardType::VictoryPoint {
            player.add_secret_victory_point();
        }

        player.dev_cards.push(card);

        Ok(card_type)
    }

    pub fn play_development_card(
        &mut self,
        card_type: shared::DevCardType,
        target: Option<shared::DevCardTarget>,
    ) -> Result<(), GameError> {
        let pid = self.current_player_id;

        {
            let player = self
                .bank
                .players
                .get(&pid)
                .ok_or(GameError::PlayerNotFound)?;
            if player.dev_card_played_this_turn {
                return Err(GameError::InvalidAction);
            }
        }

        let mut card = {
            let player = self
                .bank
                .players
                .get_mut(&pid)
                .ok_or(GameError::PlayerNotFound)?;
            let idx = player
                .dev_cards
                .iter()
                .position(|c| c.get_type() == card_type && c.can_play())
                .ok_or(GameError::InvalidAction)?;
            player.dev_cards.remove(idx)
        };

        card.play(self, &target);

        let player = self
            .bank
            .players
            .get_mut(&pid)
            .ok_or(GameError::PlayerNotFound)?;
        player.dev_cards.push(card);
        player.dev_card_played_this_turn = true;

        Ok(())
    }
    pub fn move_robber(&mut self, hex_coords: Coordinates) -> Result<(), GameError> {
        info!("Robber moved to {:?}", hex_coords);
        self.robber.pos = hex_coords;
        Ok(())
    }

    pub fn steal_card(
        &mut self,
        thief_id: Uuid,
        victim_id: Uuid,
    ) -> Result<Option<shared::ResourceType>, GameError> {
        let victim = self
            .bank
            .players
            .get_mut(&victim_id)
            .ok_or(GameError::PlayerNotFound)?;
        if let Some(res_type) = victim.resources.take_random_card() {
            let thief = self
                .bank
                .players
                .get_mut(&thief_id)
                .ok_or(GameError::PlayerNotFound)?;
            thief.resources.add(res_type, 1);
            info!(
                "Player {} stole resource from Player {}",
                thief_id, victim_id
            );
            return Ok(Some(res_type));
        }
        Ok(None)
    }

    pub fn current_player_id(&self) -> Uuid {
        self.current_player_id
    }

    pub fn get_player_info(&self, player_id: Uuid) -> Option<&Player> {
        self.bank.players.get(&player_id)
    }

    pub fn current_player_mut(&mut self) -> &mut Player {
        self.bank
            .players
            .get_mut(&self.current_player_id)
            .expect("Current player index must always be valid")
    }
    pub fn reset_order(&mut self)
    {
       self.set_current_player_index(0).unwrap()
    }
    pub fn set_current_player_index(&mut self, new_index: usize) -> Result<(), String> {
        if new_index < self.player_count {
            self.current_player_index = new_index;
            self.current_player_id = self.players[self.current_player_index].id;
            Ok(())
        } else {
            Err(format!("New index {} out of reach {}", new_index, self.player_count))
        }
    }

    pub fn add_player_with_colour(&mut self, player_id: Uuid) {
        if self.bank.players.contains_key(&player_id) {
            return;
        }
        let all_colors = vec![
            ('b', "Player1"),
            ('r', "Player2"),
            ('g', "Player3"),
            ('w', "Player4")
        ];

        let taken_colors: Vec<char> = self.players.iter()
            .map(|p| p.colour)
            .collect();

        let mut available: Vec<(char, &str)> = all_colors.into_iter()
            .filter(|(c, _)| !taken_colors.contains(c))
            .collect();

        if let Some(pos) = self.get_random_index(available.len()) {
            let (color, default_name) = available.remove(pos);

            let player = Player::new(player_id, default_name, color);
            self.players.push(player.clone());
            self.bank.players.insert(player_id, player);
        }
    }
    pub fn remove_player(&mut self, player_id: Uuid) -> Result<(), String> {
        if self.bank.players.remove(&player_id).is_some() {
            self.players.retain(|p| p.id != player_id);
            Ok(())
        } else {
            Err("Player not found".to_string())
        }
    }
    fn get_random_index(&self, len: usize) -> Option<usize> {
        if len == 0 { return None; }
        use rand::Rng;
        let mut rng = rand::thread_rng();
        Some(rng.gen_range(0..len))
    }
    pub fn has_player_won(&mut self, player_id: Uuid) -> bool
    {
        if let Some(player) = self.bank.players.get(&player_id) {
            if player.get_total_victory_points() >= 10 {
                self.game_over = true;
            }
            player.get_total_victory_points() >= 10
        } else {
            false
        }
    }
    pub fn player_secret_victory_points(&self, player_id: Uuid) -> u8
    {
        if let Some(player) = self.bank.players.get(&player_id) {
            player.get_secret_victory_points()
        } else {
            0
        }
    }
}
