-- Adds a consent model to tracking: a tracked user must accept a request
-- before the tracker gets any visibility into their location. Existing rows
-- predate this model and were already established under the old
-- no-consent-needed behavior, so they're backfilled as 'accepted' rather
-- than retroactively breaking currently-tracked relationships.
ALTER TABLE user_tracking ADD COLUMN status TEXT NOT NULL DEFAULT 'accepted';
ALTER TABLE user_tracking ALTER COLUMN status DROP DEFAULT;

ALTER TABLE user_tracking ADD CONSTRAINT chk_tracking_status CHECK (status IN ('pending', 'accepted'));

-- Replaces idx_user_tracking_tracked_user_id: every new lookup by
-- tracked_user_id (incoming requests, followers) also filters on status,
-- and a composite index still serves the old tracked_user_id-only access
-- pattern (the FK RESTRICT check) via its leading column.
DROP INDEX idx_user_tracking_tracked_user_id;
CREATE INDEX idx_user_tracking_tracked_user_id_status ON user_tracking (tracked_user_id, status);
