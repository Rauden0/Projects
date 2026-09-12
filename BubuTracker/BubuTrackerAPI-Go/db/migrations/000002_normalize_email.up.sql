-- Normalize any pre-existing data first: a CHECK constraint validates every
-- existing row when added, so without this the ALTER TABLE below would fail
-- outright if this migration ever ran against a non-empty table.
UPDATE users SET email = lower(email) WHERE email <> lower(email);

-- The application layer normalizes emails to lowercase before every write
-- and lookup; this constraint makes that a guarantee instead of a
-- convention, so a future write path can't silently reintroduce
-- case-sensitive duplicates or lookup misses.
ALTER TABLE users ADD CONSTRAINT chk_email_lowercase CHECK (email = lower(email));
