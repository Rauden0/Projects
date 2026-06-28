use actix::prelude::*;
use uuid::Uuid;
use shared::ClientRequest;

#[derive(Message)]
#[rtype(result = "()")]
pub struct Connect {
    pub addr: Recipient<ServerMessage>,
    pub player_id: Uuid,
}

#[derive(Message)]
#[rtype(result = "()")]
pub struct Disconnect {
    pub player_id: Uuid,
}

#[derive(Message)]
#[rtype(result = "()")]
pub struct ClientActorMessage {
    pub player_id: Uuid,
    pub req: ClientRequest,
}

#[derive(Message, Clone)]
#[rtype(result = "()")]
pub struct ServerMessage(pub String); 