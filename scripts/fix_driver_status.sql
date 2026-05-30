-- Fix: Approved = 1 in the DriverStatus enum (Pending=0, Approved=1, Rejected=2, Suspended=3)
UPDATE drivers SET "Status" = 1 WHERE "UserId" = (SELECT "Id" FROM users WHERE "Email" = 'admin@wasel.ma');

-- Verify
SELECT d."Id", d."Status", u."Email" FROM drivers d JOIN users u ON d."UserId" = u."Id" WHERE u."Email" = 'admin@wasel.ma';
