-- name: GetTracking :one
SELECT * FROM user_tracking WHERE tracker_id = $1 AND tracked_user_id = $2;

-- name: GetTrackedUsers :many
SELECT u.*
FROM user_tracking t
JOIN users u ON u.id = t.tracked_user_id
WHERE t.tracker_id = $1 AND t.status = 'accepted'
ORDER BY u.email;

-- name: GetIncomingTrackingRequests :many
SELECT u.*
FROM user_tracking t
JOIN users u ON u.id = t.tracker_id
WHERE t.tracked_user_id = $1 AND t.status = 'pending'
ORDER BY u.email;

-- name: GetOutgoingTrackingRequests :many
-- Profile only; no locations join (consent not granted yet).
SELECT u.*
FROM user_tracking t
JOIN users u ON u.id = t.tracked_user_id
WHERE t.tracker_id = $1 AND t.status = 'pending'
ORDER BY u.email;

-- name: GetFollowers :many
SELECT u.*
FROM user_tracking t
JOIN users u ON u.id = t.tracker_id
WHERE t.tracked_user_id = $1 AND t.status = 'accepted'
ORDER BY u.email;

-- name: AddTracking :exec
INSERT INTO user_tracking (tracker_id, tracked_user_id, status)
VALUES ($1, $2, 'pending');

-- name: AcceptTracking :execrows
-- Only pending→accepted; 0 rows means no such pending request.
UPDATE user_tracking
SET status = 'accepted'
WHERE tracker_id = $1 AND tracked_user_id = $2 AND status = 'pending';

-- name: RemoveTracking :exec
DELETE FROM user_tracking WHERE tracker_id = $1 AND tracked_user_id = $2;
