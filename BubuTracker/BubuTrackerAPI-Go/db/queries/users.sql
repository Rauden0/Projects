-- name: GetUserByID :one
SELECT * FROM users WHERE id = $1;

-- name: GetUserByAuth0SubjectID :one
SELECT * FROM users WHERE auth0_subject_id = $1;

-- name: GetUserByEmail :one
SELECT * FROM users WHERE email = $1;

-- name: UpsertUserByAuth0Subject :one
-- Creates the user on first sign-in, or converges the cached email on every
-- later one. ON CONFLICT makes this safe under concurrent first-sign-in
-- requests for the same subject: whichever call loses the race still gets
-- back the winning row instead of a unique-violation error. first_name and
-- last_name are only applied on insert, so a later Auth0 claim never
-- overwrites a name the user has since edited via UpdateUserProfile.
INSERT INTO users (auth0_subject_id, email, first_name, last_name)
VALUES ($1, $2, $3, $4)
ON CONFLICT (auth0_subject_id) DO UPDATE
SET email = EXCLUDED.email
RETURNING *;

-- name: UpdateUserProfile :one
-- Partial update: a NULL argument leaves the existing column value
-- untouched, so this single atomic statement replaces a read-modify-write
-- round trip (and the lost-update race that pattern invites).
UPDATE users
SET first_name = COALESCE(sqlc.narg(first_name), first_name),
    last_name = COALESCE(sqlc.narg(last_name), last_name)
WHERE id = sqlc.arg(id)
RETURNING *;
