use actix_web::{web, HttpRequest, HttpResponse, Error, get};
use actix_web_actors::ws;
use actix::Addr;
use log::info;
use serde::Deserialize;
use uuid::Uuid;
use crate::lobby::Lobby;
use crate::network::ws_worker::WsWorker;
#[derive(Deserialize)]
pub struct AuthQuery {
    pub id: Option<Uuid>,
}
#[get("/ws")]
pub async fn ws_index(
    req: HttpRequest,
    stream: web::Payload,
    lobby_addr: web::Data<Addr<Lobby>>,
    query: web::Query<AuthQuery>
) -> Result<HttpResponse, Error> {
    let player_id = query.id.unwrap_or_else(Uuid::new_v4);
    info!("player_id: {}", player_id);
    let worker = WsWorker::new(player_id, lobby_addr.get_ref().clone());
    ws::start(worker, &req, stream)
}