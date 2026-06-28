use std::collections::HashSet;
use uuid::Uuid;
use shared::Resources;

/// A pending trade offer
#[derive(Debug, Clone)]
pub struct PendingTrade {
    #[allow(dead_code)]
    pub offer_id: u64,
    pub proposer_id: Uuid,  // Connection ID
    pub target_player_id: Option<Uuid>,  // If Some, only this player can accept
    pub offering: Resources,
    pub requesting: Resources,
    pub declined_by: HashSet<Uuid>,  // Connection IDs of players who declined
}
