DROP INDEX IF EXISTS idx_user_tracking_tracked_user_id_status;
CREATE INDEX idx_user_tracking_tracked_user_id ON user_tracking (tracked_user_id);

ALTER TABLE user_tracking DROP CONSTRAINT IF EXISTS chk_tracking_status;
ALTER TABLE user_tracking DROP COLUMN IF EXISTS status;
