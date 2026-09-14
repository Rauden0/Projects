ALTER TABLE users ADD COLUMN marker_color TEXT NOT NULL DEFAULT '#FF9800';

ALTER TABLE users ADD CONSTRAINT chk_marker_color CHECK (marker_color IN (
    '#FF9800', -- orange
    '#E91E63', -- pink
    '#8E24AA', -- purple
    '#3949AB', -- indigo
    '#00897B', -- teal
    '#43A047', -- green
    '#F4511E', -- deep orange
    '#6D4C41'  -- brown
));
