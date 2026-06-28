use sqlx::{postgres::PgPool, Row};
use serde_json::Value;
use crate::game::entities::game_instance::GameInstance;

#[derive(Clone)]
pub struct GameRepository {
    pool: PgPool,
}

impl GameRepository {
    pub fn new(pool: PgPool) -> Self {
        Self { pool }
    }

    pub async fn save_game_instance(&self, instance: &GameInstance) -> Result<(), Box<dyn std::error::Error>> {
        let status = if instance.turn_manager.game_over { "FINISHED" } else { "ACTIVE" };

        let state_json = serde_json::to_value(instance)?;

        sqlx::query(
            r#"
            INSERT INTO games (id, status, state_json)
            VALUES ($1, $2, $3)
            ON CONFLICT (id) DO UPDATE SET
                state_json = EXCLUDED.state_json,
                status = EXCLUDED.status,
                updated_at = CURRENT_TIMESTAMP
            "#
        )
            .bind(&instance.id)
            .bind(status)
            .bind(state_json)
            .execute(&self.pool)
            .await?;

        Ok(())
    }

    pub async fn load_all_active(&self) -> Result<Vec<GameInstance>, Box<dyn std::error::Error>> {
        let rows = sqlx::query("SELECT state_json FROM games WHERE status != 'FINISHED'")
            .fetch_all(&self.pool)
            .await?;

        let mut instances = Vec::new();
        for row in rows {
            let json_value: Value = row.get("state_json");

            let inst: GameInstance = serde_json::from_value(json_value)?;
            instances.push(inst);
        }
        Ok(instances)
    }
    pub async fn delete_game_instance(&self, game_id: &str) -> Result<(), Box<dyn std::error::Error>> {
        sqlx::query("DELETE FROM games WHERE id = $1")
            .bind(game_id)
            .execute(&self.pool)
            .await?;
        Ok(())
    }
}