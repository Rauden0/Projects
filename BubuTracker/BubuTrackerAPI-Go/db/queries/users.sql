-- name: GetUserByID :one
SELECT * FROM users WHERE id = $1;

-- name: GetUserByAuth0SubjectID :one
SELECT * FROM users WHERE auth0_subject_id = $1;

-- name: GetUserByEmail :one
SELECT * FROM users WHERE email = $1;

-- name: CreateUser :one
INSERT INTO users (auth0_subject_id, email, first_name, last_name)
VALUES ($1, $2, $3, $4)
RETURNING *;

-- name: UpdateUser :one
UPDATE users
SET email = $2,
    first_name = $3,
    last_name = $4
WHERE id = $1
RETURNING *;
