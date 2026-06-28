use std::collections::HashMap;

use rand::Rng;
use serde::{Deserialize, Serialize};
pub(crate) use shared::ResourceType;
use shared::ResourceType::{Brick, Ore, Sheep, Wheat, Wood};
use shared::Resources;

#[derive(Debug, Clone, Eq, PartialEq, Serialize, Deserialize)]
pub struct ResourceSet {
    pub(crate) amounts: HashMap<ResourceType, u32>,
}

impl ResourceSet {
    pub fn new() -> Self {
        Self {
            amounts: HashMap::new(),
        }
    }

    pub fn amount_of(&self, res: ResourceType) -> u32 {
        *self.amounts.get(&res).unwrap_or(&0)
    }

    pub fn can_pay(&self, cost: &ResourceSet) -> bool {
        for (res, amount) in &cost.amounts {
            if self.amount_of(*res) < *amount {
                return false;
            }
        }
        true
    }

    pub fn take(&mut self, res: ResourceType, amount: u32) {
        let entry = self.amounts.entry(res).or_insert(0);
        *entry -= amount;
    }

    pub fn add(&mut self, res: ResourceType, amount: u32) {
        if res == ResourceType::Desert {
            return;
        }
        *self.amounts.entry(res).or_insert(0) += amount;
    }

    pub fn add_set(&mut self, set: &ResourceSet) {
        for (res, amount) in &set.amounts {
            self.add(*res, *amount);
        }
    }

    pub fn take_set(&mut self, set: &ResourceSet) {
        for (res, &amt) in &set.amounts {
            self.take(*res, amt);
        }
    }

    pub fn get_cards_total(&mut self) -> u32 {
        self.amounts.values().sum()
    }

    pub fn take_random_card(&mut self) -> Option<ResourceType> {
        let total = self.get_cards_total();
        if total == 0 {
            return None;
        }

        let mut rng = rand::thread_rng();
        let mut choice = rng.gen_range(0..total);

        for (resource, amount) in self.amounts.iter_mut() {
            if *amount == 0 {
                continue;
            }

            if choice < *amount {
                *amount -= 1;
                return Some(*resource);
            } else {
                choice -= *amount;
            }
        }

        None
    }


}
impl From<&ResourceSet> for Resources {
    fn from(r: &ResourceSet) -> Self {
        Resources {
            brick: *r.amounts.get(&Brick).unwrap_or(&0) as u8,
            lumber: *r.amounts.get(&Wood).unwrap_or(&0) as u8,
            wool: *r.amounts.get(&Sheep).unwrap_or(&0) as u8,
            grain: *r.amounts.get(&Wheat).unwrap_or(&0) as u8,
            ore: *r.amounts.get(&Ore).unwrap_or(&0) as u8,
        }
    }
}
