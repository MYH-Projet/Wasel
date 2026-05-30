-- Verify user exists first
SELECT "Id", "Email" FROM users WHERE "Email" = 'admin@wasel.ma';

-- Insert driver profile for admin user (Status=2 means Approved)
INSERT INTO drivers ("Id", "UserId", "LicenseNumber", "VehicleType", "Status", "IsAvailable", "CreatedAt", "UpdatedAt")
SELECT gen_random_uuid(), "Id", 'ADMIN-LIC-001', 'Car', 2, true, now(), now()
FROM users
WHERE "Email" = 'admin@wasel.ma'
ON CONFLICT DO NOTHING;

-- Verify driver was created
SELECT d."Id", d."UserId", d."Status", u."Email"
FROM drivers d JOIN users u ON d."UserId" = u."Id"
WHERE u."Email" = 'admin@wasel.ma';
