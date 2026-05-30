SELECT d."Id", d."Status", d."UserId", u."Email"
FROM drivers d
JOIN users u ON d."UserId" = u."Id";

SELECT '---USERS---';
SELECT "Id", "Email", "KeycloakId" FROM users;
