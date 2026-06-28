# PV281 - Colonist

## Description

This is a project for a college course in which we recreate the popular online game [Colonist](https://colonist.io/).

## Game

Colonist is a strategic resource‑management and settlement‑building game inspired by classic board games. Players gather resources, build roads and settlements, expand their territory, and strategically trade with others to gain advantages. Through smart planning and negotiation, players race to earn victory points — the first to reach 10 wins.

## Rules and Game Settings
You can learn about the rules of the game and game settings [here](docs/game_rules.md).

## 🛠 Prerequisites

Before running the project, ensure you have the following installed:
* **Rust & Cargo** (stable)
* **Trunk**: The WASM web assets bundler (`cargo install trunk`)
* **PostgreSQL**: Database instance running on port 5432

---

## 🚀 Execution Guide (Where & How to Run)

To run the full stack, you will need **three separate terminal windows/tabs** open at the same time:

### Step 1: Start the Database (PostgreSQL)
* **Where:** Any terminal window on your system.
* **How:** Run this command to start a Docker container that matches your `.env` credentials:
    ```bash
    docker run --name catan-db \
      -e POSTGRES_USER=admin \
      -e POSTGRES_PASSWORD=heslo123 \
      -e POSTGRES_DB=catan \
      -p 5432:5432 -d postgres
    ```

### Step 2: Start the Backend Server
* **Where:** Open a new terminal in the **root directory** of the project (where your `.env` file and main `Cargo.toml` are located).
* **How:** 
  1. Make sure your `.env` file contains the posgtres connection string: 
  ```env
  RUST_LOG="debug"
  DATABASE_URL=postgres://admin:heslo123@localhost:5432/catan
  ```
  2. Run the server using the package flag:
     ```bash
     cd colonist
     cargo run --package backend
     ```
*Leave this terminal running. It handles all game logic and database saves.*

### Step 3: Start the Frontend Client
* **Where:** Open a third terminal and navigate into the `frontend/` subdirectory.
* **How:**
    ```bash
    cd colonist
    cd frontend
    trunk serve
    ```
*Note: This will compile the code to WebAssembly. Once it says "Successfully built", the game is live at **http://localhost:8080**.*

---

## 🏗 Project Architecture

* **📂 `shared/`**: Common logic used by both ends. This includes `ServerMessage` enums, `PlayerInfo` structs, and resource management logic.
* **📂 `backend/`**: The Actix-web server, the core game engine (dice, hexes, robber), and the PostgreSQL repository.
* **📂 `frontend/`**: The Yew client, UI components, and the Canvas-based game board.