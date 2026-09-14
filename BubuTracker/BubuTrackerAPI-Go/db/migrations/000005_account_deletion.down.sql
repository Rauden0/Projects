ALTER TABLE user_tracking DROP CONSTRAINT user_tracking_tracker_id_fkey;
ALTER TABLE user_tracking DROP CONSTRAINT user_tracking_tracked_user_id_fkey;

ALTER TABLE user_tracking
    ADD CONSTRAINT user_tracking_tracker_id_fkey
        FOREIGN KEY (tracker_id) REFERENCES users (id) ON DELETE RESTRICT,
    ADD CONSTRAINT user_tracking_tracked_user_id_fkey
        FOREIGN KEY (tracked_user_id) REFERENCES users (id) ON DELETE RESTRICT;
