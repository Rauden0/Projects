use crate::game::entities::building::{EdgeBuilding, VertexBuilding};
use rand::seq::SliceRandom;
use serde::{Deserialize, Serialize};
use serde_with::serde_as;
use shared::{BoardInfo, BuildingInfo, HexInfo, PortInfo, ResourceType};
use std::collections::HashMap;
use std::collections::HashSet;
use std::iter::repeat_n;
use uuid::Uuid;

pub type Coordinates = (i32, i32);

#[derive(Debug, Clone, Serialize, Deserialize)]
pub struct Hex {
    pub coord: Coordinates,

    pub resource: ResourceType,
    pub number: u8,

    pub adjacent_vertices: [Coordinates; 6],
}

#[derive(Debug, Serialize, Deserialize, Clone)]
pub struct Vertex {
    pub coord: Coordinates,

    pub building: Option<VertexBuilding>,
    pub owner: Option<Uuid>,

    pub adjacent_edges: HashSet<Coordinates>,
}

#[derive(Debug, Serialize, Deserialize, Clone)]
pub struct Edge {
    pub coord: Coordinates,

    pub building: Option<EdgeBuilding>,
    pub owner: Option<Uuid>,

    pub adjacent_vertices: (Coordinates, Coordinates),
}

#[derive(Debug, Clone, Copy, PartialEq, Serialize, Deserialize)]
pub enum PortType {
    ThreeToOne,
    TwoToOne(ResourceType),
}

impl From<&PortType> for shared::PortType {
    fn from(p: &PortType) -> Self {
        match p {
            PortType::ThreeToOne => shared::PortType::ThreeToOne,
            PortType::TwoToOne(res) => shared::PortType::TwoToOne(*res),
        }
    }
}
#[derive(Debug, Clone, Serialize, Deserialize)]
pub struct Port {
    pub coord: [Coordinates; 2],
    pub port_type: PortType,
}
impl From<&Port> for PortInfo {
    fn from(p: &Port) -> Self {
        PortInfo {
            vertices: [
                (p.coord[0].0, p.coord[0].1),
                (p.coord[1].0, p.coord[1].1),
            ],
            port_type: (&p.port_type).into(),
        }
    }
}
#[serde_as]
#[derive(Debug, Default, Serialize, Deserialize, Clone)]
pub struct Board {
    #[serde_as(as = "Vec<(_, _)>")]
    pub hexes: HashMap<Coordinates, Hex>,
    #[serde_as(as = "Vec<(_, _)>")]
    pub vertices: HashMap<Coordinates, Vertex>,
    #[serde_as(as = "Vec<(_, _)>")]
    pub edges: HashMap<Coordinates, Edge>,
    #[serde_as(as = "Vec<(_, _)>")]
    pub ports: HashMap<Coordinates, Port>,
}

impl Board {
    pub fn new() -> Self {
        Self::new_standard_board()
    }

    fn get_hex_neighbours() -> [Coordinates; 6] {
        [(1, 0), (0, 1), (-1, 1), (-1, 0), (0, -1), (1, -1)]
    }

    pub fn get_adjacent_vertices(hex_coord: Coordinates) -> [Coordinates; 6] {
        let (q, r) = hex_coord;
        let base = (q * 3, r * 3);

        let neighbours = Self::get_hex_neighbours();
        let mut vertices = [base; 6];

        for i in 0..6 {
            vertices[i] = (
                base.0 + neighbours[i].0 + neighbours[(i + 1) % 6].0,
                base.1 + neighbours[i].1 + neighbours[(i + 1) % 6].1,
            );
        }

        vertices
    }

    // maybe useless
    pub fn get_adjacent_edges(hex_coord: Coordinates) -> [Coordinates; 6] {
        let (q, r) = hex_coord;
        let base = (q * 2, r * 2);

        let neighbours = Self::get_hex_neighbours();
        let mut edges = [base; 6];

        for i in 0..6 {
            edges[i] = (
                base.0 + neighbours[i].0,
                base.1 + neighbours[i].1,
            );
        }

        edges
    }

    pub fn edge_key(a: Coordinates, b: Coordinates) -> Coordinates {
        ((a.0 + b.0) / 3, (a.1 + b.1) / 3)
    }

    pub fn generate_from_layout(resources: Vec<ResourceType>, numbers: Vec<u8>) -> Self {
        let mut board = Board::default();


        // ---------- generate hex coordinates ----------
        let radius = 2;
        let mut hex_coords = Vec::new();
        for q in -radius..=radius {
            for r in -radius..=radius {
                if ((q + r) as i32).abs() <= radius {
                    hex_coords.push((q, r));
                }
            }
        }

        // ---------- create hexes ----------
        let mut res_iter = resources.into_iter();
        let mut num_iter = numbers.into_iter();

        for &hex_coord in &hex_coords {
            let resource = res_iter.next().unwrap_or(ResourceType::Desert);
            let number = num_iter.next().unwrap_or(0);

            board.hexes.insert(
                hex_coord,
                Hex {
                    coord: hex_coord,
                    resource,
                    number,
                    adjacent_vertices: Self::get_adjacent_vertices(hex_coord),
                },
            );
        }


        for &hex_coord in &hex_coords {
            let vertices = Self::get_adjacent_vertices(hex_coord);

            for i in 0..6 {
                let a = vertices[i];
                let b = vertices[(i + 1) % 6];
                let edge_coord = Self::edge_key(a, b);

                // ---------- edge ----------
                board.edges.entry(edge_coord).or_insert_with(|| Edge {
                    coord: edge_coord,
                    building: None,
                    owner: None,
                    adjacent_vertices: (a, b),
                });

                // ---------- vertex a ----------
                board.vertices
                    .entry(a)
                    .or_insert_with(|| Vertex {
                        coord: a,
                        building: None,
                        owner: None,
                        adjacent_edges: HashSet::new(),
                    })
                    .adjacent_edges
                    .insert(edge_coord);

                // ---------- vertex b ----------
                board.vertices
                    .entry(b)
                    .or_insert_with(|| Vertex {
                        coord: b,
                        building: None,
                        owner: None,
                        adjacent_edges: HashSet::new(),
                    })
                    .adjacent_edges
                    .insert(edge_coord);
            }
        }

        board
    }


    pub fn test_layout() -> (Vec<ResourceType>, Vec<u8>) { // standard counts: wood 4, sheep 4, wheat 4, brick 3, ore 3, desert 1 => 19 hexes
        use ResourceType::*;
        let mut resources = Vec::with_capacity(19);
        resources.extend(repeat_n(Wood, 4));
        resources.extend(repeat_n(Sheep, 4));
        resources.extend(repeat_n(Wheat, 4));
        resources.extend(repeat_n(Brick, 3));
        resources.extend(repeat_n(Ore, 3));
        resources.push(Desert);

        let numbers = vec![5, 2, 6, 3, 8, 10, 9, 12, 11, 4, 8, 10, 9, 4, 5, 6, 3, 11, 0]; // fixed order

        (resources, numbers)
    }

    pub fn random_layout() -> (Vec<ResourceType>, Vec<u8>) {
        let (mut resources, mut numbers) = Self::test_layout();

        resources.pop();
        numbers.pop();

        let mut rng = rand::thread_rng();
        resources.shuffle(&mut rng);
        numbers.shuffle(&mut rng);

        resources.push(ResourceType::Desert);
        numbers.push(0);

        (resources, numbers)
    }

    pub fn add_ports(&mut self) {
        use PortType::*;

        let port_coords: [[Coordinates; 2]; 9] = [
            [(2, -7), (4, -8)],
            [(7, -8), (8, -7)],
            [(8, -4), (7, -2)],
            [(5, 2), (4, 4)],
            [(1, 7), (-1, 8)],
            [(-4, 8), (-5, 7)],
            [(-7, 5), (-8, 4)],
            [(-7, -1), (-8, 1)],
            [(-4, -4), (-2, -5)],
        ];

        let mut port_types = vec![
            TwoToOne(ResourceType::Brick),
            TwoToOne(ResourceType::Wood),
            TwoToOne(ResourceType::Sheep),
            TwoToOne(ResourceType::Wheat),
            TwoToOne(ResourceType::Ore),
            ThreeToOne,
            ThreeToOne,
            ThreeToOne,
            ThreeToOne,
        ];

        let mut rng = rand::thread_rng();
        port_types.shuffle(&mut rng);

        let mut ports_map = HashMap::new();
        for (coords, port_type) in port_coords.iter().zip(port_types.into_iter()) {
            let port = Port {
                coord: *coords,
                port_type,
            };

            ports_map.insert(coords[0], port.clone());
            ports_map.insert(coords[1], port);
        }

        self.ports = ports_map;
    }

    /// Returns unique ports (deduplicated - each port appears once, not twice per vertex)
    pub fn unique_ports(&self) -> Vec<Port> {
        use std::collections::HashSet;

        let mut seen: HashSet<[Coordinates; 2]> = HashSet::new();
        let mut result: Vec<Port> = Vec::new();

        for port in self.ports.values() {
            // Create canonical key by ordering the coordinates
            let a = port.coord[0];
            let b = port.coord[1];
            let key = if a <= b { [a, b] } else { [b, a] };

            if seen.insert(key) {
                result.push(port.clone());
            }
        }

        result
    }

    pub fn new_standard_board() -> Self {
        let (resources, numbers) = Self::random_layout();
        let mut board = Self::generate_from_layout(resources, numbers);
        board.add_ports();
        board
    }

    pub fn is_buildable_vertex(&self, pos: Coordinates) -> bool {
        let Some(vertex) = self.vertices.get(&pos) else {
            return false;
        };

        for edge_coord in &vertex.adjacent_edges {
            let Some(edge) = self.edges.get(edge_coord) else {
                return false;
            };

            for &vertex_coord in &[edge.adjacent_vertices.0, edge.adjacent_vertices.1] {
                if let Some(vertex) = self.vertices.get(&vertex_coord) {
                    if vertex.building.is_some() {
                        return false;
                    }
                }
            }
        }

        true
    }

    pub fn is_vertex_connected_to_player(&self, pos: Coordinates, player_id: Uuid) -> bool {
        self.vertices
            .get(&pos)
            .map(|v| {
                v.adjacent_edges.iter().any(|&ek| {
                    self.edges.get(&ek).map(|e| {
                        match e.owner {
                            Some(id) => id == player_id,
                            None => false,
                        }
                    }).unwrap_or(false)
                })
            })
            .unwrap_or(false)
    }

    pub fn is_edge_connected_to_player(&self, pos: Coordinates, player_id: Uuid) -> bool {
        let Some(edge) = self.edges.get(&pos) else {
            return false;
        };

        let vertices = [edge.adjacent_vertices.0, edge.adjacent_vertices.1];

        vertices.iter().any(|&vcoord| {
            if let Some(v) = self.vertices.get(&vcoord) {
                if let Some(id) = v.owner {
                    if id == player_id { 
                        return true;
                    }
                }

                if v.adjacent_edges.iter().any(|&ek| {
                    if let Some(e) = self.edges.get(&ek) {
                        if let Some(id) = e.owner {
                            return id == player_id;
                        }
                    }
                    false
                }) {
                    return true;
                }
            }

            false
        })
    }

    pub fn build_vertex(&mut self, player_id: Uuid, pos: Coordinates, building: VertexBuilding) {
        if let Some(vertex) = self.vertices.get_mut(&pos) {
            vertex.building = Some(building);
            vertex.owner = Some(player_id);
        }
    }

    pub fn build_edge(&mut self, player_id: Uuid, pos: Coordinates, building: EdgeBuilding) {
        if let Some(edge) = self.edges.get_mut(&pos) {
            edge.building = Some(building);
            edge.owner = Some(player_id);
        }
    }

    pub fn calculate_longest_road(&self, player_id: Uuid) -> usize {
        let player_edges: HashSet<_> = self
            .edges
            .iter()
            .filter(|(_, e)| e.owner == Some(player_id))
            .map(|(c, _)| *c)
            .collect();

        fn dfs(
            board: &Board,
            current_vertex: Coordinates,
            player_id: Uuid,
            player_edges: &HashSet<Coordinates>,
            visited_edges: &mut HashSet<Coordinates>,
        ) -> usize {
            let mut max_len = 0;

            if let Some(vertex) = board.vertices.get(&current_vertex) {
                for &edge_coord in &vertex.adjacent_edges {
                    if !player_edges.contains(&edge_coord) || visited_edges.contains(&edge_coord) {
                        continue;
                    }

                    let edge = &board.edges[&edge_coord];
                    let next_vertex = if edge.adjacent_vertices.0 == current_vertex {
                        edge.adjacent_vertices.1
                    } else {
                        edge.adjacent_vertices.0
                    };

                    if let Some(v) = board.vertices.get(&next_vertex) {
                        if let Some(owner) = v.owner {
                            if owner != player_id {
                                continue;
                            }
                        }
                    }

                    visited_edges.insert(edge_coord);
                    let len =
                        1 + dfs(board, next_vertex, player_id, player_edges, visited_edges);
                    visited_edges.remove(&edge_coord);

                    max_len = max_len.max(len);
                }
            }

            max_len
        }

        let mut max_length = 0;

        let all_vertices: HashSet<_> = player_edges
            .iter()
            .flat_map(|e| {
                let edge = &self.edges[e];
                [edge.adjacent_vertices.0, edge.adjacent_vertices.1]
            })
            .collect();

        for &vertex in &all_vertices {
            let mut visited_edges = HashSet::new();
            let length = dfs(self, vertex, player_id, &player_edges, &mut visited_edges);
            max_length = max_length.max(length);
        }

        max_length
    }

    pub fn to_info(&self, robber_pos: (i32, i32)) -> BoardInfo {
        let hexes = self.hexes.iter()
            .map(|(coords, hex)| HexInfo {
                q: coords.0,
                r: coords.1,
                resource: hex.resource,
                number: hex.number,
            })
            .collect();

        let mut settlements = Vec::new();
        let mut cities = Vec::new();

        for (coords, vertex) in &self.vertices {
            if let Some(building) = &vertex.building {
                let info = BuildingInfo {
                    player_id: vertex.owner.unwrap_or(Uuid::nil()),
                    x: coords.0,
                    y: coords.1,
                };

                match building {
                    VertexBuilding::Settlement => settlements.push(info),
                    VertexBuilding::City => cities.push(info),
                }
            }
        }

        let roads = self.edges.iter()
            .filter_map(|(coords, edge)| {
                edge.owner.map(|pid| BuildingInfo {
                    player_id: pid,
                    x: coords.0,
                    y: coords.1,
                })
            })
            .collect();

        let ports = self.unique_ports().into_iter()
            .map(|port| {
                PortInfo {
                    vertices: port.coord,
                    port_type: match port.port_type {
                        PortType::ThreeToOne => shared::PortType::ThreeToOne,
                        PortType::TwoToOne(res) => shared::PortType::TwoToOne(res),
                    },
                }
            })
            .collect();

        BoardInfo {
            hexes,
            settlements,
            cities,
            roads,
            robber_pos,
            ports,
        }
    }

    pub fn get_number_of_buildings(&self, pid: Uuid) -> usize
    {
        self.vertices.values()
            .filter(|v| v.owner == Some(pid))
            .count()
    }
    pub fn get_number_of_roads(&self, pid: Uuid) -> usize
    {
        self.edges.values()
            .filter(|e| e.owner == Some(pid))
            .count()
    }
}

