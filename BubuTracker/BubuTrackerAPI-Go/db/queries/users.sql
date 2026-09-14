-- name: GetUserByID :one
SELECT * FROM users WHERE id = $1;

-- name: GetUserByAuth0SubjectID :one
SELECT * FROM users WHERE auth0_subject_id = $1;

-- name: GetUserByEmail :one
SELECT * FROM users WHERE email = $1;

-- name: UpsertUserByAuth0Subject :one
-- Names only applied on INSERT so Auth0 never overwrites an edited profile.
INSERT INTO users (auth0_subject_id, email, first_name, last_name)
VALUES ($1, $2, $3, $4)
ON CONFLICT (auth0_subject_id) DO UPDATE
SET email = EXCLUDED.email
RETURNING *;

-- name: UpdateUserProfile :one
-- NULL args leave the existing column value unchanged.
UPDATE users
SET first_name = COALESCE(sqlc.narg(first_name), first_name),
    last_name = COALESCE(sqlc.narg(last_name), last_name),
    marker_color = COALESCE(sqlc.narg(marker_color), marker_color)
WHERE id = sqlc.arg(id)
RETURNING *;

-- name: DeleteUser :execrows
-- Cascades to locations and every user_tracking edge (migration 000005).
DELETE FROM users WHERE id = $1;
