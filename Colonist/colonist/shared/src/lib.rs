use serde::{Deserialize, Serialize};
use uuid::Uuid;

fn default_player_count() -> usize { 4 }

#[derive(Debug, Clone, Serialize, Deserialize, PartialEq, Eq)]
#[serde(tag = "type", content = "payload")]
#[serde(rename_all = "SCREAMING_SNAKE_CASE")]
pub enum ClientRequest {
    CreateGame {
        #[serde(default = "default_player_count")]
        player_count: usize
    },
    JoinGame {
        game_id: String,
    },
    LeaveGame
    {
        game_id: String,
    },
    RollDice,
    EndTurn,
    BuildSettlement { x: i32, y: i32 },
    BuildCity { x: i32, y: i32 },
    BuildRoad { x1: i32, y1: i32 },
    BuyDevelopmentCard,
    PlayDevCard {
        card: DevCardType,
        target: Option<DevCardTarget>,
    },
    TradeOffer {
        /// If None, trade is open to all players. If Some, only that player can accept.
        target_player_id: Option<Uuid>,
        offer: Resources,
        request: Resources,
    },
    TradeResponse {
        offer_id: u64,
        accept: bool,
    },
    CancelTrade {
        offer_id: u64,
    },
    ShowMap,
    RequestResources,
    Chat {
        message: String,
    },
    GetLobbyList,
    MoveRobber {
        q: i32,
        r: i32,
    },
    StealFromPlayer {
        victim_id: Uuid,
    },
    DiscardCards {
        resources: Resources,
    },
    BankTrade {
        give: ResourceType,
        receive: ResourceType,
    },
    YearOfPlentyChoice {
        resource1: ResourceType,
        resource2: ResourceType,
    },
    MonopolyChoice {
        resource: ResourceType,
    },
    /// DEBUG: Auto-complete initial placement phase for all players
    DebugAutoInitialPlacement,
}

#[derive(Debug, Serialize, Clone, Deserialize)]
#[serde(tag = "event", content = "data")]
#[serde(rename_all = "SCREAMING_SNAKE_CASE")]
pub enum ServerMessage {
    Joined {
        player_id: Uuid,
        game_id: String,
    },
    GameStarted {
        your_player_id: Uuid,
        players: Vec<PlayerInfo>,
        board: BoardState,
        game_phase: GamePhase,
    },
    /// Update all players' info (victory points, dev cards, etc)
    PlayersUpdate {
        players: Vec<PlayerInfo>,
    },
    LobbyUpdate {
        games: Vec<(String, usize, usize)>,
    },
    DiceRolled {
        player_id: Uuid,
        dice_1: u8,
        dice_2: u8,
        /// Number of players who must discard (only set when 7 is rolled)
        #[serde(default)]
        discards_pending: usize,
    },
    ResourceUpdate {
        player_id: Uuid,
        resources: Resources,
    },
    Built {
        player_id: Uuid,
        #[serde(rename = "type")]
        structure_type: String,
        coords: Vec<i32>,
    },
    NextTurn {
        player_id: Uuid,
    },
    PhaseChanged {
        new_phase: GamePhase,
    },
    ChatMessage {
        player_id: Uuid,
        text: String,
    },
    DevCardBought {
        player_id: Uuid,
        card_type: DevCardType,
    },
    DevCardPlayed {
        player_id: Uuid,
        card_type: DevCardType,
    },
    Error {
        message: String,
    },
    RobberMoved {
        player_id: Uuid,
        new_q: i32,
        new_r: i32,
    },
    PlayerRobbed {
        thief_id: Uuid,
        victim_id: Uuid,
        resource: Option<ResourceType>,
    },
    MustDiscardCards {
        player_id: Uuid,
        count: usize,
    },
    CardsDiscarded {
        player_id: Uuid,
        count: usize,
    },
    MustMoveRobber {
        player_id: Uuid,
    },
    MustPlaceRoads {
        player_id: Uuid,
        roads_remaining: u8,
    },
    MustChooseYearOfPlentyResources {
        player_id: Uuid,
    },
    YearOfPlentyResourcesReceived {
        player_id: Uuid,
        resource1: ResourceType,
        resource2: ResourceType,
    },
    MustChooseMonopolyResource {
        player_id: Uuid,
    },
    MonopolyResourcesStolen {
        player_id: Uuid,
        resource: ResourceType,
        total_stolen: u32,
    },
    CanRobPlayers {
        player_ids: Vec<Uuid>,
    },
    BankTradeCompleted {
        player_id: Uuid,
        gave: ResourceType,
        received: ResourceType,
    },
    PortsUpdate {
        player_id: Uuid,
        ports: Vec<PortType>,
    },
    /// A player has proposed a trade
    TradeProposed {
        offer_id: u64,
        proposer_id: Uuid,
        /// If Some, only this player can accept
        target_player_id: Option<Uuid>,
        offering: Resources,
        requesting: Resources,
    },
    /// A trade was completed successfully
    TradeCompleted {
        offer_id: u64,
        proposer_id: Uuid,
        accepter_id: Uuid,
        /// What the proposer gave
        proposer_gave: Resources,
        /// What the accepter gave
        accepter_gave: Resources,
    },
    /// A trade offer was cancelled by the proposer
    TradeCancelled {
        offer_id: u64,
    },
    /// A trade offer was declined (only sent to proposer)
    TradeDeclined {
        offer_id: u64,
        decliner_id: Uuid,
    },
    FullStateSync {
        player_id: Uuid,
        players: Vec<PlayerInfo>,
        board: BoardInfo,
        game_phase: GamePhase,
        current_turn_player_id: Uuid,
        robber_pos: (i32, i32),
        last_dice_roll: Option<(u8, u8)>,
    },
    PlayerWon { player_id: Uuid, secret_victory_points: u8 },
    PlayerSecretVictoryPointsUpdated{secret_victory_points: i32},

    Left { player_id: Uuid, game_id: String },
}

#[derive(Debug, Serialize, Deserialize, Clone, PartialEq, Eq)]
#[serde(rename_all = "SCREAMING_SNAKE_CASE")]
pub enum DevCardType {
    Knight, VictoryPoint, RoadBuilding, Monopoly, YearOfPlenty,
}

#[derive(Debug, Serialize, Deserialize, Clone, PartialEq, Eq)]
pub struct DevCardTarget {
    pub hex_index: Option<usize>,
    pub resource_type: Option<ResourceType>,
}

#[derive(Debug, Serialize, Deserialize, Clone, Copy, PartialEq, Eq, Hash)]
#[serde(rename_all = "SCREAMING_SNAKE_CASE")]
pub enum ResourceType {
    Brick,
    Wood, Wool,
    Wheat, Ore, Sheep, Desert
}

#[derive(Debug, Serialize, Deserialize, Clone, Default, PartialEq, Eq)]
pub struct Resources {
    pub brick: u8,
    pub lumber: u8,
    pub wool: u8,
    pub grain: u8,
    pub ore: u8,
}

#[derive(Debug, Serialize, Deserialize, Clone, PartialEq, Eq)]
#[serde(rename_all = "SCREAMING_SNAKE_CASE")]
pub enum GamePhase {
    WaitingForPlayers,
    InitialPlacementRound1,
    InitialPlacementRound2,
    RegularPlay,
}

/// Port types for maritime trading
#[derive(Debug, Serialize, Deserialize, Clone, Copy, PartialEq, Eq)]
#[serde(rename_all = "SCREAMING_SNAKE_CASE")]
pub enum PortType {
    /// Generic 3:1 port - trade 3 of any resource for 1 of any other
    ThreeToOne,
    /// Specific 2:1 port - trade 2 of specific resource for 1 of any other
    TwoToOne(ResourceType),
}

/// Port info with vertex coordinates and type (for board state)
#[derive(Debug, Serialize, Deserialize, Clone, PartialEq, Eq)]
pub struct PortInfo {
    /// Two vertex coordinates that define the port edge
    pub vertices: [(i32, i32); 2],
    /// The type of port (3:1 or 2:1 for specific resource)
    pub port_type: PortType,
}

#[derive(Debug, Serialize, Deserialize, Clone, PartialEq, Eq)]
pub struct BoardInfo{
    pub hexes: Vec<HexInfo>,
    pub settlements: Vec<BuildingInfo>,
    pub cities: Vec<BuildingInfo>,
    pub roads: Vec<BuildingInfo>,
    pub robber_pos: (i32, i32),
    pub ports: Vec<PortInfo>
}

#[derive(Debug, Serialize, Deserialize, Clone, PartialEq, Eq)]
pub struct PlayerInfo {
    pub player_id: Uuid,
    pub name: String,
    pub color: String,
    pub victory_points: u8,
    pub dev_cards: Vec<DevCardType>,
    /// Ports this player has access to (from settlements/cities on port vertices)
    pub ports: Vec<PortType>,
    pub resources: Resources,
    pub knights_played: usize,
    pub roads_count: usize,
    pub has_longest_road: bool,
    pub has_largest_army: bool,
}

#[derive(Debug, Serialize, Deserialize, Clone, PartialEq, Eq)]
pub struct BoardState {
    pub hexes: Vec<HexInfo>,
    pub settlements: Vec<BuildingInfo>,
    pub cities: Vec<BuildingInfo>,
    pub roads: Vec<BuildingInfo>,
    pub robber_pos: (i32, i32),
    /// Ports on the board with their vertex coordinates and types
    pub ports: Vec<PortInfo>,
}

#[derive(Debug, Serialize, Deserialize, Clone, PartialEq, Eq)]
pub struct HexInfo {
    pub q: i32,
    pub r: i32,
    pub resource: ResourceType,
    pub number: u8,
}

#[derive(Debug, Serialize, Deserialize, Clone, PartialEq, Eq)]
pub struct BuildingInfo {
    pub player_id: Uuid,
    pub x: i32,
    pub y: i32,
}