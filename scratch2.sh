#!/bin/bash
CLIENT_TOKEN=$(curl -s -X POST "http://localhost:8080/auth/realms/wasel/protocol/openid-connect/token" -H "Content-Type: application/x-www-form-urlencoded" -d "client_id=wasel-api&username=client@wasel.ma&password=client123&grant_type=password" | grep -o '"access_token":"[^"]*' | grep -o '[^"]*$')
DELIVERY_ID="575b80aa-f821-4d62-86e4-046138eac75f"
echo "Sending POST /api/reviews for delivery $DELIVERY_ID"
curl -s -X POST "http://localhost:5000/api/reviews" -H "Authorization: Bearer $CLIENT_TOKEN" -H "Content-Type: application/json" -d "{\"deliveryId\": \"$DELIVERY_ID\", \"rating\": 5, \"comment\": \"Excellent service!\"}"
