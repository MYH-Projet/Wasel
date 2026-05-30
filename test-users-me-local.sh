#!/bin/bash
TOKEN=$(curl -s -X POST "http://localhost:8080/auth/realms/wasel/protocol/openid-connect/token" \
  -H "Content-Type: application/x-www-form-urlencoded" \
  -d "client_id=wasel-api" \
  -d "username=client@wasel.ma" \
  -d "password=client123" \
  -d "grant_type=password" | grep -o '"access_token":"[^"]*"' | cut -d'"' -f4)

echo "Testing PATCH /api/users/me with valid phone..."
curl -s -o /dev/null -w "%{http_code}" -X PATCH "http://localhost:5000/api/users/me" \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{"phone": "0612345678"}'
echo ""

echo "Testing PATCH /api/users/me with invalid phone..."
curl -s -o /dev/null -w "%{http_code}" -X PATCH "http://localhost:5000/api/users/me" \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{"phone": "123"}'
echo ""

echo "Testing PATCH /api/users/me/preferences CLIENT..."
curl -s -o /dev/null -w "%{http_code}" -X PATCH "http://localhost:5000/api/users/me/preferences" \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{"activeAppMode": "CLIENT", "preferredMode": "CLIENT"}'
echo ""

echo "Testing PATCH /api/users/me/preferences DRIVER sans profil..."
curl -s -o /dev/null -w "%{http_code}" -X PATCH "http://localhost:5000/api/users/me/preferences" \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{"activeAppMode": "DRIVER", "preferredMode": "CLIENT"}'
echo ""
