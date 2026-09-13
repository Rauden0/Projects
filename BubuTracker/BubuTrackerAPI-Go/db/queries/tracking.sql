-- name: GetTracking :one
SELECT * FROM user_tracking WHERE tracker_id = $1 AND tracked_user_id = $2;

-- name: GetTrackedUsers :many
-- Only users whose tracking request has been accepted - a pending request
-- grants no visibility into the target's location yet.
SELECT u.*
FROM user_tracking t
JOIN users u ON u.id = t.tracked_user_id
WHERE t.tracker_id = $1 AND t.status = 'accepted'
ORDER BY u.email;

-- name: GetIncomingTrackingRequests :many
-- Pending requests from other users to track the current user - the
-- consent inbox they accept or reject from.
SELECT u.*
FROM user_tracking t
JOIN users u ON u.id = t.tracker_id
WHERE t.tracked_user_id = $1 AND t.status = 'pending'
ORDER BY u.email;

-- name: GetFollowers :many
-- Users who are currently, with consent, tracking the current user - so
-- they have ongoing visibility into (and can revoke) who has access, not
-- just at request time.
SELECT u.*
FROM user_tracking t
JOIN users u ON u.id = t.tracker_id
WHERE t.tracked_user_id = $1 AND t.status = 'accepted'
ORDER BY u.email;

-- name: AddTracking :exec
-- Creates a pending request rather than an active tracking edge; the
-- tracked user must accept it (AcceptTracking) before the tracker gets any
-- location visibility.
INSERT INTO user_tracking (tracker_id, tracked_user_id, status)
VALUES ($1, $2, 'pending');

-- name: AcceptTracking :execrows
-- Only transitions a still-pending request, so accepting a request that's
-- already accepted (or was rejected/removed) affects zero rows instead of
-- silently no-op'ing on the wrong state - the caller can tell the two apart.
UPDATE user_tracking
SET status = 'accepted'
WHERE tracker_id = $1 AND tracked_user_id = $2 AND status = 'pending';

-- name: RemoveTracking :exec
-- Deletes the (tracker, tracked) edge regardless of its status, so this one
-- statement serves four call sites: the tracker canceling their own pending
-- request or stopping active tracking, and the tracked user rejecting a
-- pending request or revoking consent already given.
DELETE FROM user_tracking WHERE tracker_id = $1 AND tracked_user_id = $2;
