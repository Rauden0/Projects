use crate::lobby::GameInstance;
use shared::ResourceType::{Brick, Ore, Sheep, Wheat, Wood};
use shared::ServerMessage;
use uuid::Uuid;

pub fn handle_discard_cards(pid: Uuid, resources: shared::Resources, game: &mut GameInstance) -> Result<ServerMessage, String> {
    if !game.pending_discards.contains(&pid) {
        return Err("You are not required to discard right now".to_string());
    }

    let tm = &mut game.turn_manager;
    let mut discard_set = crate::game::entities::resources::ResourceSet::new();
    discard_set.add(Brick, resources.brick as u32);
    discard_set.add(Wood, resources.lumber as u32);
    discard_set.add(Sheep, resources.wool as u32);
    discard_set.add(Wheat, resources.grain as u32);
    discard_set.add(Ore, resources.ore as u32);

    match tm.bank.players.get_mut(&pid) {
        Some(player) => {
            if player.resources.can_pay(&discard_set) {
                player.resources.take_set(&discard_set);

                game.pending_discards.remove(&pid);
                let count = resources.brick + resources.lumber + resources.wool + resources.grain + resources.ore;

                Ok(ServerMessage::CardsDiscarded {
                    player_id: pid,
                    count: count as usize,
                })
            } else {
                Err("Not enough resources to discard".to_string())
            }
        }
        None => Err("Player not found".to_string())
    }
}

pub fn handle_buy_dev_card(pid: Uuid, game: &mut GameInstance) -> Result<ServerMessage, String> {
    let tm = &mut game.turn_manager;
    if pid != tm.current_player_id { return Err("Wait for your turn!".to_string()); }

    tm.buy_dev_card()
        .map(|card_type| {
            ServerMessage::DevCardBought {
                player_id: pid,
                card_type,
            }
        })
        .map_err(|e| format!("{:?}", e))
}

pub fn handle_play_dev_card(pid: Uuid, card: shared::DevCardType, target: Option<shared::DevCardTarget>, game: &mut GameInstance) -> Result<ServerMessage, String> {
    let tm = &mut game.turn_manager;
    if pid != tm.current_player_id { return Err("Wait for your turn!".to_string()); }

    tm.play_development_card(card.clone(), target)
        .map(|_| {
            // If a knight was played, the player must move the robber
            if card == shared::DevCardType::Knight {
                game.knight_mover = Some(pid);
            }
            // If Road Builder was played, the player gets 2 free roads
            if card == shared::DevCardType::RoadBuilding {
                game.free_roads_remaining = 2;
            }
            // If Year of Plenty was played, the player must choose 2 resources
            if card == shared::DevCardType::YearOfPlenty {
                game.year_of_plenty_pending = Some(pid);
            }
            // If Monopoly was played, the player must choose a resource type
            if card == shared::DevCardType::Monopoly {
                game.monopoly_pending = Some(pid);
            }
            ServerMessage::DevCardPlayed {
                player_id: pid,
                card_type: card,
            }
        })
        .map_err(|e| format!("{:?}", e))
}

pub fn handle_move_robber(pid: Uuid, q: i32, r: i32, game: &mut GameInstance) -> Result<ServerMessage, String> {
    let tm = &mut game.turn_manager;
    if pid != tm.current_player_id { return Err("Wait for your turn!".to_string()); }

    tm.move_robber((q, r))
        .map(|_| {
            game.seven_roller = None; // Reset seven_roller logic
            game.knight_mover = None; // Reset knight_mover logic
            ServerMessage::RobberMoved {
                player_id: pid,
                new_q: q,
                new_r: r,
            }
        })
        .map_err(|e| format!("{:?}", e))
}

pub fn handle_steal_from_player(pid: Uuid, victim_id: Uuid, game: &mut GameInstance) -> Result<ServerMessage, String> {
    let tm = &mut game.turn_manager;

    tm.steal_card(pid, victim_id)
        .map(|resource| {
            ServerMessage::PlayerRobbed {
                thief_id: pid,
                victim_id,
                resource,
            }
        })
        .map_err(|e| format!("{:?}", e))
}

pub fn handle_year_of_plenty_choice(
    pid: Uuid,
    resource1: shared::ResourceType,
    resource2: shared::ResourceType,
    game: &mut GameInstance,
) -> Result<ServerMessage, String> {
    if game.year_of_plenty_pending != Some(pid) {
        return Err("You don't have a pending Year of Plenty".to_string());
    }

    let tm = &mut game.turn_manager;

    tm.bank.game_resources.take(resource1, 1);
    tm.bank.game_resources.take(resource2, 1);

    {
        let player = tm
            .bank
            .players
            .get_mut(&pid)
            .ok_or_else(|| "Player not found".to_string())?;

        player.resources.add(resource1, 1);
        player.resources.add(resource2, 1);
    }

    game.year_of_plenty_pending = None;

    Ok(ServerMessage::YearOfPlentyResourcesReceived {
        player_id: pid,
        resource1,
        resource2,
    })
}

pub fn handle_monopoly_choice(
    pid: Uuid,
    resource: shared::ResourceType,
    game: &mut GameInstance,
) -> Result<ServerMessage, String> {
    if game.monopoly_pending != Some(pid) {
        return Err("You don't have a pending Monopoly".to_string());
    }

    let tm = &mut game.turn_manager;

    // Collect all resources of this type from all other players
    let total_stolen = tm.bank.collect_resource_from_all_to_player(pid, resource)
        .map_err(|e| format!("{:?}", e))?;

    game.monopoly_pending = None;

    Ok(ServerMessage::MonopolyResourcesStolen {
        player_id: pid,
        resource,
        total_stolen,
    })
}