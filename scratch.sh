#!/bin/bash
CLIENT_TOKEN=$(curl -s -X POST "http://localhost:8080/auth/realms/wasel/protocol/openid-connect/token" -H "Content-Type: application/x-www-form-urlencoded" -d "client_id=wasel-api&username=client@wasel.ma&password=client123&grant_type=password" | grep -o '"access_token":"[^"]*' | grep -o '[^"]*$')
echo Token: ${CLIENT_TOKEN:0:10}...

DRIVER_ME_RESPONSE=$(curl -s -X GET "http://localhost:5000/api/drivers/me" -H "Authorization: Bearer $CLIENT_TOKEN")
echo "Response: $DRIVER_ME_RESPONSE"
