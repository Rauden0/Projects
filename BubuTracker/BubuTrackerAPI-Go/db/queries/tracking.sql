-- name: GetTracking :one
SELECT * FROM user_tracking WHERE tracker_id = $1 AND tracked_user_id = $2;

-- name: GetTrackedUsers :many
SELECT u.*
FROM user_tracking t
JOIN users u ON u.id = t.tracked_user_id
WHERE t.tracker_id = $1
ORDER BY u.email;

-- name: AddTracking :exec
INSERT INTO user_tracking (tracker_id, tracked_user_id)
VALUES ($1, $2);

-- name: RemoveTracking :exec
DELETE FROM user_tracking WHERE tracker_id = $1 AND tracked_user_id = $2;
