-- name: UpsertLocation :one
INSERT INTO locations (user_id, latitude, longitude, updated_at)
VALUES ($1, $2, $3, now())
ON CONFLICT (user_id)
DO UPDATE SET latitude = $2, longitude = $3, updated_at = now()
RETURNING *;

-- name: GetTrackedLocations :many
-- LEFT JOIN so users without a location still appear; accepted edges only.
SELECT
    u.id AS user_id,
    u.email,
    u.first_name,
    u.last_name,
    u.marker_color,
    l.latitude,
    l.longitude,
    l.updated_at
FROM user_tracking t
JOIN users u ON u.id = t.tracked_user_id
LEFT JOIN locations l ON l.user_id = t.tracked_user_id
WHERE t.tracker_id = $1 AND t.status = 'accepted'
ORDER BY u.email;
