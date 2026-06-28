use serde::{Deserialize, Serialize};
use shared::ResourceType;
use crate::game::entities::board::{Coordinates, Board};

#[derive(Debug, Serialize, Deserialize, Clone)]
pub struct Robber {
    pub pos: Coordinates,
}

impl Robber {
    pub fn new(board: &Board) -> Self {
        for (coord, hex) in &board.hexes {
            if hex.resource == ResourceType::Desert {
                return Self {
                    pos: *coord,
                };
            }
        }

        panic!("No desert hex found on the board");
    }

    pub fn glyph(&self) -> char {
        'R'
    }

}

impl Default for Robber {
    fn default() -> Self {
        Self { pos: (0, 0) } // some placeholder
    }
}
