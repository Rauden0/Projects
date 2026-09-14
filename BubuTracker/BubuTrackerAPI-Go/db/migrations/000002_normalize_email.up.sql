-- Lowercase existing emails before adding the CHECK (validates all rows).
UPDATE users SET email = lower(email) WHERE email <> lower(email);

ALTER TABLE users ADD CONSTRAINT chk_email_lowercase CHECK (email = lower(email));
