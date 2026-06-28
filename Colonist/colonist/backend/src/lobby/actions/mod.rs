use crate::lobby::{GameInstance, Lobby};
use actix::prelude::Context;
use shared::{ClientRequest, ServerMessage};
use uuid::Uuid;

pub mod construction;
pub mod development;
mod lobby;
pub mod trading;
pub mod turn_control;

pub fn handle_lobby_action(
    lobby: &mut Lobby,
    pid: Uuid,
    req: ClientRequest,
    ctx: &mut Context<Lobby>,
) {
    match req {
        ClientRequest::CreateGame { player_count } => {
            lobby.handle_create_game(pid, player_count, ctx);
        }

        ClientRequest::JoinGame { game_id } => lobby.handle_join_game(pid, game_id, ctx),

        ClientRequest::LeaveGame { game_id } => {
            lobby.handle_leave_game(pid, game_id, ctx);
        }

        ClientRequest::GetLobbyList => lobby.broadcast_lobby_status(),
        _ => {}
    }
}

pub fn handle_game_request(
    pid: Uuid,
    req: ClientRequest,
    game: &mut GameInstance,
) -> Result<ServerMessage, String> {
    match req {
        ClientRequest::RollDice => game.handle_roll_dice(pid),
        ClientRequest::EndTurn => game.handle_end_turn(pid),

        ClientRequest::BuildSettlement { x, y } => game.handle_build_settlement(pid, x, y),
        ClientRequest::BuildCity { x, y } => game.handle_build_city(pid, x, y),
        ClientRequest::BuildRoad { x1, y1 } => game.handle_build_road(pid, x1, y1),

        ClientRequest::BankTrade { give, receive } => {
            trading::handle_bank_trade(pid, give, receive, game)
        }
        ClientRequest::TradeOffer {
            target_player_id,
            offer,
            request,
        } => trading::handle_trade_offer(pid, target_player_id, offer, request, game),
        ClientRequest::TradeResponse { offer_id, accept } => {
            trading::handle_trade_response(pid, offer_id, accept, game)
        }
        ClientRequest::CancelTrade { offer_id } => {
            trading::handle_cancel_trade(pid, offer_id, game)
        }

        ClientRequest::DiscardCards { resources } => {
            development::handle_discard_cards(pid, resources, game)
        }
        ClientRequest::BuyDevelopmentCard => development::handle_buy_dev_card(pid, game),
        ClientRequest::PlayDevCard { card, target } => {
            development::handle_play_dev_card(pid, card, target, game)
        }
        ClientRequest::MoveRobber { q, r } => development::handle_move_robber(pid, q, r, game),
        ClientRequest::StealFromPlayer { victim_id } => development::handle_steal_from_player(pid, victim_id, game),
        ClientRequest::YearOfPlentyChoice { resource1, resource2 } => development::handle_year_of_plenty_choice(pid, resource1, resource2, game),
        ClientRequest::MonopolyChoice { resource } => development::handle_monopoly_choice(pid, resource, game),

        ClientRequest::Chat { message } => Ok(ServerMessage::ChatMessage {
            player_id: pid,
            text: message,
        }),

        _ => Err("Action not implemented yet".to_string()),
    }
}