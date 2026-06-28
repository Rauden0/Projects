use actix::Actor;
use actix_web::{middleware, web, App, HttpServer};
use log::{error, info};
use std::{env, io};
use dotenv::dotenv;
use sqlx::postgres::PgPoolOptions;
use crate::lobby::Lobby;
use crate::repository::game_repository::GameRepository;

mod game;
mod errors;
mod network;
mod lobby;
mod repository;

#[actix_web::main]
async fn main() -> Result<(), io::Error> {
    dotenv().ok();
    env_logger::init_from_env(env_logger::Env::new().default_filter_or("info"));

    info!("COLONIST.RS Server starting...");
    let database_url = env::var("DATABASE_URL")
        .expect("DATABASE_URL must be set in the .env file");

    let pool = PgPoolOptions::new()
        .max_connections(10)
        .connect(&database_url)
        .await
        .expect("Failed to connect to PostgreSQL");

    sqlx::query(
        r#"
        CREATE TABLE IF NOT EXISTS games (
            id TEXT PRIMARY KEY,
            status TEXT NOT NULL,
            state_json JSONB NOT NULL,
            updated_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP
        );
        "#
        )
        .execute(&pool)
        .await
        .expect("Failed to initialize database schema");

    let repo = GameRepository::new(pool);
    let recovered_games = repo.load_all_active().await.unwrap_or_else(|e| {
        error!("Critical error during game recovery: {}", e);
        Vec::new()
    });

    let games_count = recovered_games.len();

    let lobby = Lobby::new(repo, recovered_games).start();

    info!("COLONIST.RS Server listening at 127.0.0.1:8080");
    info!("Recovered {} active games from database", games_count);

    HttpServer::new(move || {
        App::new()
            .wrap(middleware::Logger::default())
            .app_data(web::Data::new(lobby.clone()))
            .service(network::routes::ws_index)
    })
        .bind(("127.0.0.1", 8080))?
        .run()
        .await
}