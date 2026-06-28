use uuid::Uuid;
use shared::{ServerMessage, GamePhase, ResourceType};
use shared::ResourceType::{Brick, Ore, Sheep, Wheat, Wood};
use std::collections::HashSet;
use shared::ServerMessage::TradeCancelled;
use crate::game::entities::pending_trade::PendingTrade;
use crate::lobby::GameInstance;

pub fn handle_bank_trade(pid: Uuid, give: ResourceType, receive: ResourceType, game: &mut GameInstance) -> Result<ServerMessage, String> {
    let tm = &mut game.turn_manager;
    let is_initial_phase = matches!(game.phase, GamePhase::InitialPlacementRound1 | GamePhase::InitialPlacementRound2);

    if pid != tm.current_player_id {
        return Err("Wait for your turn!".to_string());
    }

    if is_initial_phase {
        return Err("Cannot trade during initial placement!".to_string());
    }

    let ratio = tm.bank.players.get(&pid)
        .map(|player| tm.bank.best_ratio(player, give))
        .unwrap_or(4);

    // Build ResourceSets for the trade
    let mut gives = crate::game::entities::resources::ResourceSet::new();
    gives.add(give, ratio);

    let mut takes = crate::game::entities::resources::ResourceSet::new();
    takes.add(receive, 1);

    tm.bank.trade_with_bank(pid, gives, takes)
        .map(|_| {
            ServerMessage::BankTradeCompleted {
                player_id: pid,
                gave: give,
                received: receive,
            }
        })
        .map_err(|e| format!("{:?}", e))
}

pub fn handle_trade_offer(
    pid: Uuid,
    target_player_id: Option<Uuid>,
    offer: shared::Resources,
    request: shared::Resources,
    game: &mut GameInstance
) -> Result<ServerMessage, String> {
    let tm = &mut game.turn_manager;
    let is_initial_phase = matches!(game.phase, GamePhase::InitialPlacementRound1 | GamePhase::InitialPlacementRound2);

    if pid != tm.current_player_id { return Err("Wait for your turn!".to_string()); }
    if is_initial_phase { return Err("Cannot trade during initial placement!".to_string()); }

    // Check if proposer has the resources
    let can_afford = tm.bank.players.get(&pid)
        .map(|player| {
            let res = &player.resources;
            res.amount_of(Brick) >= offer.brick as u32 &&
                res.amount_of(Wood) >= offer.lumber as u32 &&
                res.amount_of(Sheep) >= offer.wool as u32 &&
                res.amount_of(Wheat) >= offer.grain as u32 &&
                res.amount_of(Ore) >= offer.ore as u32
        })
        .unwrap_or(false);

    if !can_afford {
        return Err("You don't have enough resources for this trade".to_string());
    }

    let offer_id = game.next_trade_id;
    game.next_trade_id += 1;

    let pending_trade = PendingTrade {
        offer_id,
        proposer_id: pid,
        target_player_id,
        offering: offer.clone(),
        requesting: request.clone(),
        declined_by: HashSet::new(),
    };
    game.pending_trades.insert(offer_id, pending_trade);

    Ok(ServerMessage::TradeProposed {
        offer_id,
        proposer_id: pid,
        target_player_id,
        offering: offer,
        requesting: request,
    })
}

pub fn handle_trade_response(pid: Uuid, offer_id: u64, accept: bool, game: &mut GameInstance) -> Result<ServerMessage, String> {
    let trade = game.pending_trades.get(&offer_id).cloned();
    let tm = &mut game.turn_manager;

    match trade {
        // Already finished 
        None => Ok(TradeCancelled {
            offer_id,
        }),
        Some(trade) => {
            if trade.proposer_id == pid {
                return Err("You cannot respond to your own trade".to_string());
            } else if trade.target_player_id.is_some() && trade.target_player_id != Some(pid) {
                return Err("This trade was not offered to you".to_string());
            } else if trade.declined_by.contains(&pid) {
                return Err("You already declined this trade".to_string());
            }

            if !accept {
                // DECLINE LOGIC
                if let Some(pending_trade) = game.pending_trades.get_mut(&offer_id) {
                    pending_trade.declined_by.insert(pid);
                }

                // Check if all eligible players declined
                let eligible_count = if trade.target_player_id.is_some() {
                    1
                } else {
                    game.player_ids.len() - 1
                };

                let declined_count = game.pending_trades.get(&offer_id)
                    .map(|t| t.declined_by.len())
                    .unwrap_or(0);

                if declined_count >= eligible_count {
                    game.pending_trades.remove(&offer_id);
                    // Poznámka: Odeslání Cancelled zprávy řeší handlers.rs,
                    // pokud zjistí, že trade zmizel z mapy, nebo to můžeme nechat tady
                    // Ale handlers.rs na to nemají trigger.
                    // V tomto případě vrátíme TradeDeclined a handlers.rs musí zkontrolovat,
                    // jestli byl trade smazán.
                }

                Ok(ServerMessage::TradeDeclined {
                    offer_id,
                    decliner_id: pid,
                })
            } else {
                // ACCEPT LOGIC
                let proposer_can_afford = tm.bank.players.get(&trade.proposer_id)
                    .map(|player| {
                        let res = &player.resources;
                        res.amount_of(Brick) >= trade.offering.brick as u32 &&
                            res.amount_of(Wood) >= trade.offering.lumber as u32 &&
                            res.amount_of(Sheep) >= trade.offering.wool as u32 &&
                            res.amount_of(Wheat) >= trade.offering.grain as u32 &&
                            res.amount_of(Ore) >= trade.offering.ore as u32
                    })
                    .unwrap_or(false);

                let accepter_can_afford = tm.bank.players.get(&pid)
                    .map(|player| {
                        let res = &player.resources;
                        res.amount_of(Brick) >= trade.requesting.brick as u32 &&
                            res.amount_of(Wood) >= trade.requesting.lumber as u32 &&
                            res.amount_of(Sheep) >= trade.requesting.wool as u32 &&
                            res.amount_of(Wheat) >= trade.requesting.grain as u32 &&
                            res.amount_of(Ore) >= trade.requesting.ore as u32
                    })
                    .unwrap_or(false);

                if !proposer_can_afford {
                    game.pending_trades.remove(&offer_id);
                    Err("Proposer no longer has the resources".to_string())
                } else if !accepter_can_afford {
                    Err("You don't have enough resources for this trade".to_string())
                } else {
                    // Build resource sets
                    let mut proposer_gives = crate::game::entities::resources::ResourceSet::new();
                    proposer_gives.add(Brick, trade.offering.brick as u32);
                    proposer_gives.add(Wood, trade.offering.lumber as u32);
                    proposer_gives.add(Sheep, trade.offering.wool as u32);
                    proposer_gives.add(Wheat, trade.offering.grain as u32);
                    proposer_gives.add(Ore, trade.offering.ore as u32);

                    let mut accepter_gives = crate::game::entities::resources::ResourceSet::new();
                    accepter_gives.add(Brick, trade.requesting.brick as u32);
                    accepter_gives.add(Wood, trade.requesting.lumber as u32);
                    accepter_gives.add(Sheep, trade.requesting.wool as u32);
                    accepter_gives.add(Wheat, trade.requesting.grain as u32);
                    accepter_gives.add(Ore, trade.requesting.ore as u32);

                    if let Err(e) = tm.bank.collect_from_player_to_player(trade.proposer_id, pid, &proposer_gives) {
                        return Err(format!("Trade failed: {:?}", e));
                    }

                    if let Err(e) = tm.bank.collect_from_player_to_player(pid, trade.proposer_id, &accepter_gives) {
                        let _ = tm.bank.collect_from_player_to_player(pid, trade.proposer_id, &proposer_gives);
                        return Err(format!("Trade failed: {:?}", e));
                    }

                    game.pending_trades.remove(&offer_id);

                    Ok(ServerMessage::TradeCompleted {
                        offer_id,
                        proposer_id: trade.proposer_id,
                        accepter_id: pid,
                        proposer_gave: trade.offering.clone(),
                        accepter_gave: trade.requesting.clone(),
                    })
                }
            }
        }
    }
}

pub fn handle_cancel_trade(pid: Uuid, offer_id: u64, game: &mut GameInstance) -> Result<ServerMessage, String> {
    let trade = game.pending_trades.get(&offer_id);
    match trade {
        None => Err("Trade not found".to_string()),
        Some(trade) => {
            if trade.proposer_id != pid {
                Err("You can only cancel your own trades".to_string())
            } else {
                game.pending_trades.remove(&offer_id);
                Ok(ServerMessage::TradeCancelled { offer_id })
            }
        }
    }
}