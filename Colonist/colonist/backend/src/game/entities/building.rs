use crate::game::entities::resources::ResourceSet;
use shared::ResourceType;
use serde::{Serialize, Deserialize};

#[derive(Debug, Clone, Copy, PartialEq, Serialize, Deserialize)]
pub enum VertexBuilding {
    Settlement,
    City,
}

impl VertexBuilding {
    pub fn glyph(&self) -> char {
        match self {
            Self::Settlement => 'S',
            Self::City => 'C',
        }
    }

    pub fn cost(&self) -> ResourceSet {
        let mut cost = ResourceSet::new();
        match self {
            Self::Settlement => {
                cost.add(ResourceType::Wood, 1);
                cost.add(ResourceType::Brick, 1);
                cost.add(ResourceType::Sheep, 1);
                cost.add(ResourceType::Wheat, 1);
            }
            Self::City => {
                cost.add(ResourceType::Ore, 3);
                cost.add(ResourceType::Wheat, 2);
            }
        }
        cost
    }

    pub fn victory_points(&self) -> u8 {
        match self {
            Self::Settlement => 1,
            Self::City => 2,
        }
    }

    pub fn production(&self) -> u32 {
        match self {
            Self::Settlement => 1,
            Self::City => 2,
        }
    }
}

#[derive(Debug, Clone, Copy, PartialEq, Serialize, Deserialize)]
pub enum EdgeBuilding {
    Road,
}

impl EdgeBuilding {
    pub fn glyph(&self) -> char {
        'R'
    }

    pub fn cost(&self) -> ResourceSet {
        let mut cost = ResourceSet::new();
        cost.add(ResourceType::Wood, 1);
        cost.add(ResourceType::Brick, 1);
        cost
    }

    pub fn victory_points(&self) -> u8 {
        0
    }
}