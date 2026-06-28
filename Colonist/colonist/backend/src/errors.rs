use std::fmt;

#[derive(Debug, Clone, PartialEq)]
pub enum GameError {
    NotEnoughBuildingsOfThisType,
    WrongResourceRatio,
    PlayerNotFound,
    NotPlayersTurn,
    NotEnoughResources,
    InvalidPosition,
    InvalidAction,
    TradeNotFound,
    UnauthorizedTrade,
    TradeWithSelf,
}

impl fmt::Display for GameError {
    fn fmt(&self, f: &mut fmt::Formatter<'_>) -> fmt::Result {
        let msg = match self {
            GameError::NotEnoughBuildingsOfThisType => "Not enough buildings of this type",
            GameError::WrongResourceRatio => "Wrong resource ratio",
            GameError::PlayerNotFound => "Player not found",
            GameError::NotPlayersTurn => "Not player's turn",
            GameError::NotEnoughResources => "Not enough resources",
            GameError::InvalidPosition => "Invalid position",
            GameError::InvalidAction => "Invalid action",
            GameError::TradeNotFound => "Trade not found",
            GameError::UnauthorizedTrade => "Unauthorized trade",
            GameError::TradeWithSelf => "Cannot trade with self",
        };
        f.write_str(msg)
    }
}

impl std::error::Error for GameError {}