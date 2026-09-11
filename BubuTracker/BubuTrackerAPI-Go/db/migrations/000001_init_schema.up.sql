CREATE EXTENSION IF NOT EXISTS pgcrypto;

CREATE TABLE users (
    id               UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    auth0_subject_id TEXT NOT NULL UNIQUE,
    email            TEXT NOT NULL UNIQUE,
    first_name       TEXT NOT NULL DEFAULT '',
    last_name        TEXT NOT NULL DEFAULT '',
    created_at       TIMESTAMPTZ NOT NULL DEFAULT now()
);

CREATE TABLE locations (
    id         UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    user_id    UUID NOT NULL UNIQUE REFERENCES users (id) ON DELETE CASCADE,
    latitude   DOUBLE PRECISION NOT NULL,
    longitude  DOUBLE PRECISION NOT NULL,
    updated_at TIMESTAMPTZ NOT NULL DEFAULT now(),
    CONSTRAINT chk_latitude_range CHECK (latitude BETWEEN -90 AND 90),
    CONSTRAINT chk_longitude_range CHECK (longitude BETWEEN -180 AND 180)
);

CREATE TABLE user_tracking (
    tracker_id      UUID NOT NULL REFERENCES users (id) ON DELETE RESTRICT,
    tracked_user_id UUID NOT NULL REFERENCES users (id) ON DELETE RESTRICT,
    created_at      TIMESTAMPTZ NOT NULL DEFAULT now(),
    PRIMARY KEY (tracker_id, tracked_user_id),
    CONSTRAINT chk_no_self_tracking CHECK (tracker_id <> tracked_user_id)
);

CREATE INDEX idx_user_tracking_tracked_user_id ON user_tracking (tracked_user_id);
