-- The application layer normalizes emails to lowercase before every write
-- and lookup; this constraint makes that a guarantee instead of a
-- convention, so a future write path can't silently reintroduce
-- case-sensitive duplicates or lookup misses.
ALTER TABLE users ADD CONSTRAINT chk_email_lowercase CHECK (email = lower(email));
