use leptos::*;
use futures::{SinkExt, StreamExt}; // You'll need `futures` in cargo.toml
use gloo_net::websocket::{futures::WebSocket, Message};
use shared::{ClientRequest, ServerMessage};

#[derive(Clone)]
pub struct WsAction {
    pub signal: WriteSignal<Option<ClientRequest>>,
}

pub fn use_ws() -> WsAction {
    use_context::<WsAction>().expect("WsAction context should be provided")
}

pub fn provide_websocket_context() {
    let (tx, mut rx) = futures::channel::mpsc::unbounded::<ClientRequest>();
    provide_context(tx.clone());
    let state = use_context::<GameState>().expect("GameState missing");

    create_effect(move |_| {
        let Some(pid) = state.player_id.get() else { return; };
        let mut rx = rx;
        let tx_ping = tx.clone();

        let ws_url = format!("ws://127.0.0.1:8080/ws?id={}", pid);
        let ws = WebSocket::open(&ws_url).expect("Failed to connect");
        let (mut write, mut read) = ws.split();

        spawn_local(async move {
            while let Some(Ok(Message::Text(txt))) = read.next().await {
                state.process_message(txt);
            }
        });

        spawn_local(async move {
            let mut interval = gloo_timers::future::IntervalStream::new(20_000);

            loop {
                futures::select! {
                    _ = interval.next().fuse() => {
                        let _ = write.send(Message::Text("{\"type\":\"Ping\"}".to_string())).await;
                    },
                    req = rx.next().fuse() => {
                        if let Some(req) = req {
                            if let Ok(json) = serde_json::to_string(&req) {
                                let _ = write.send(Message::Text(json)).await;
                            }
                        }
                    }
                }
            }
        });
    });
}
fn handle_server_message(msg: ServerMessage) {
    match msg {
        ServerMessage::DiceRolled { dice_1, dice_2, .. } => {
            log::info!("Dice rolled: {} {}", dice_1, dice_2);
        }
        _ => log::info!("Got msg: {:?}", msg),
    }
}