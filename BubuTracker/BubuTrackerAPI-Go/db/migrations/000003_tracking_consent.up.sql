-- Default 'accepted' backfills pre-consent rows so existing tracking keeps working.
ALTER TABLE user_tracking ADD COLUMN status TEXT NOT NULL DEFAULT 'accepted';
ALTER TABLE user_tracking ALTER COLUMN status DROP DEFAULT;

ALTER TABLE user_tracking ADD CONSTRAINT chk_tracking_status CHECK (status IN ('pending', 'accepted'));

-- Composite replaces idx_user_tracking_tracked_user_id (leading column still covers FK checks).
DROP INDEX idx_user_tracking_tracked_user_id;
CREATE INDEX idx_user_tracking_tracked_user_id_status ON user_tracking (tracked_user_id, status);
