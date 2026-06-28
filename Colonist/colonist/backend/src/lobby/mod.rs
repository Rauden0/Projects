use crate::game::entities::game_instance::GameInstance;
use crate::network::message::ServerMessage;
use crate::repository::game_repository::GameRepository;
use actix::prelude::*;
use std::collections::HashMap;
use uuid::Uuid;

mod handlers;
mod actions;
mod networking;
mod game_logic;



pub struct Lobby {
    pub sessions: HashMap<Uuid, Recipient<ServerMessage>>,
    pub games: HashMap<String, GameInstance>,
    pub player_to_game: HashMap<Uuid, String>,
    pub repo: GameRepository, // Repository for persisting game instances
}

impl Actor for Lobby {
    type Context = Context<Self>;
}

impl Lobby {

    pub fn new(repo: GameRepository, recovered_games: Vec<GameInstance>) -> Self {
        let mut games = HashMap::new();
        let mut player_to_game = HashMap::new();

        for inst in recovered_games {
            let gid = inst.id.clone();
            for pid in &inst.player_ids {
                player_to_game.insert(*pid, gid.clone());
            }
            games.insert(gid, inst);
        }

        Lobby {
            sessions: HashMap::new(),
            games,
            player_to_game,
            repo, // Store the repository for future persistence calls
        }
    }
}
