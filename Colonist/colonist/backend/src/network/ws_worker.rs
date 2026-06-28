use actix::{Actor, ActorContext, AsyncContext, StreamHandler, Handler, Addr, Running};
use actix_web_actors::ws;
use std::time::{Duration, Instant};
use log::debug;
use uuid::Uuid;
use shared::ClientRequest;
use crate::lobby::Lobby;
use crate::network::message::{Connect, Disconnect, ClientActorMessage, ServerMessage};

const HEARTBEAT_INTERVAL: Duration = Duration::from_secs(90);
const CLIENT_TIMEOUT: Duration = Duration::from_secs(120);

pub struct WsWorker {
    pub id: Uuid,
    pub lobby_addr: Addr<Lobby>,
    pub hb: Instant,
}

impl WsWorker {
    pub fn new(player_id: Uuid, lobby: Addr<Lobby>) -> Self {
        Self {
            id: player_id,
            lobby_addr: lobby,
            hb: Instant::now(),
        }
    }

    fn hb(&self, ctx: &mut ws::WebsocketContext<Self>) {
        ctx.run_interval(HEARTBEAT_INTERVAL, |act, ctx| {
            if Instant::now().duration_since(act.hb) > CLIENT_TIMEOUT {
                debug!("Client {} timed out", act.id);
                act.lobby_addr.do_send(Disconnect { player_id: act.id });
                ctx.stop();
                return;
            }
            ctx.ping(b"");
        });
    }
}

impl Actor for WsWorker {
    type Context = ws::WebsocketContext<Self>;

    fn started(&mut self, ctx: &mut Self::Context) {
        self.hb(ctx);
        let addr = ctx.address();
        self.lobby_addr.do_send(Connect {
            addr: addr.recipient(),
            player_id: self.id,
        });
    }

    fn stopping(&mut self, _: &mut Self::Context) -> Running {
        self.lobby_addr.do_send(Disconnect { player_id: self.id });
        Running::Stop
    }
}

impl StreamHandler<Result<ws::Message, ws::ProtocolError>> for WsWorker {
    fn handle(&mut self, msg: Result<ws::Message, ws::ProtocolError>, ctx: &mut Self::Context) {
        match msg {
            Ok(ws::Message::Ping(msg)) => {
                self.hb = Instant::now();
                ctx.pong(&msg);
            }
            Ok(ws::Message::Pong(_)) => {
                self.hb = Instant::now();
            }
            Ok(ws::Message::Text(text)) => {
                self.hb = Instant::now();
                if text.contains("\"Ping\"") || text == "ping" {
                    return;
                }
                let m_req: Result<ClientRequest, _> = serde_json::from_str(&text);
                match m_req {
                    Ok(req) => {
                        self.lobby_addr.do_send(ClientActorMessage {
                            player_id: self.id,
                            req,
                        });
                    },
                    Err(e) => {
                        debug!("⚠ Failed to parse client JSON: {}. Text was: {}", e, text);
                    }
                }
            }
            Ok(ws::Message::Close(reason)) => {
                debug!("WS closing for player {}: {:?}", self.id, reason);
                ctx.stop();
            }
            _ => (),
        }
    }
}

impl Handler<ServerMessage> for WsWorker {
    type Result = ();

    fn handle(&mut self, msg: ServerMessage, ctx: &mut Self::Context) {
        ctx.text(msg.0);
    }
}