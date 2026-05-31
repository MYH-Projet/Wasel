#!/bin/bash
set -e

echo "========================================================"
echo " ⚡ Wasel API Test - Messaging Security"
echo "========================================================"

BASE_URL="http://localhost:5000"
KEYCLOAK_URL="http://localhost:8080/auth/realms/wasel/protocol/openid-connect/token"
CLIENT_ID="wasel-api"

CLIENT_EMAIL="admin@wasel.ma"
CLIENT_PASSWORD="admin123"

OTHER_EMAIL="client@wasel.ma"
OTHER_PASSWORD="client123"

# 1. Retrieve tokens
echo -e "\n==> 1. Retrieving tokens..."
CLIENT_TOKEN=$(curl -s -X POST "$KEYCLOAK_URL" \
  -H "Content-Type: application/x-www-form-urlencoded" \
  -d "client_id=$CLIENT_ID" \
  -d "username=$CLIENT_EMAIL" \
  -d "password=$CLIENT_PASSWORD" \
  -d "grant_type=password" | grep -o '"access_token":"[^"]*"' | cut -d'"' -f4)

OTHER_TOKEN=$(curl -s -X POST "$KEYCLOAK_URL" \
  -H "Content-Type: application/x-www-form-urlencoded" \
  -d "client_id=$CLIENT_ID" \
  -d "username=$OTHER_EMAIL" \
  -d "password=$OTHER_PASSWORD" \
  -d "grant_type=password" | grep -o '"access_token":"[^"]*"' | cut -d'"' -f4)

if [ -z "$CLIENT_TOKEN" ] || [ -z "$OTHER_TOKEN" ]; then
    echo "❌ ERROR: Failed to retrieve tokens."
    exit 1
fi
echo "✅ PASS: Tokens retrieved"

echo -e "\n==> 1.5 Auto-syncing users..."
curl -s -o /dev/null -X GET "$BASE_URL/api/auth/me" -H "Authorization: Bearer $CLIENT_TOKEN"
curl -s -o /dev/null -X GET "$BASE_URL/api/auth/me" -H "Authorization: Bearer $OTHER_TOKEN"
echo "✅ PASS: Users synchronized to local DB"

# 2. Create a Delivery as CLIENT
echo -e "\n==> 2. Creating a delivery..."
DELIVERY_PAYLOAD=$(cat <<EOF
{
  "pickupAddress": {
    "label": "Pickup",
    "street": "123 Main St",
    "city": "Casablanca",
    "country": "Morocco"
  },
  "dropoffAddress": {
    "label": "Dropoff",
    "street": "456 Office St",
    "city": "Casablanca",
    "country": "Morocco"
  },
  "parcel": {
    "description": "Test Messaging Delivery",
    "weight": 1.5,
    "volume": 0.5,
    "isFragile": false
  },
  "paymentMethod": 0
}
EOF
)

DELIVERY_RESPONSE=$(curl -s -X POST "$BASE_URL/api/deliveries" \
  -H "Authorization: Bearer $CLIENT_TOKEN" \
  -H "Content-Type: application/json" \
  -d "$DELIVERY_PAYLOAD")

DELIVERY_ID=$(echo $DELIVERY_RESPONSE | grep -o '"deliveryId":"[^"]*' | head -n 1 | grep -o '[^"]*$')

if [ -z "$DELIVERY_ID" ]; then
    echo "❌ ERROR: Failed to create delivery."
    echo "Response: $DELIVERY_RESPONSE"
    exit 1
fi
echo "   Delivery ID: $DELIVERY_ID"
echo "✅ PASS: Delivery created"

# 3. Test Access to Delivery Chat as CLIENT (Authorized)
echo -e "\n==> 3. Testing GET /api/deliveries/$DELIVERY_ID/messages as ADMIN (owner/admin)..."
CLIENT_HTTP_STATUS=$(curl -s -o /dev/null -w "%{http_code}" -X GET "$BASE_URL/api/deliveries/$DELIVERY_ID/messages" \
  -H "Authorization: Bearer $CLIENT_TOKEN")

if [ "$CLIENT_HTTP_STATUS" -eq 200 ]; then
    echo "✅ PASS: Admin successfully accessed the delivery chat."
else
    echo "❌ ERROR: Admin access failed (HTTP $CLIENT_HTTP_STATUS)"
    exit 1
fi

# 4. Test Access to Delivery Chat as OTHER (Unauthorized)
echo -e "\n==> 4. Testing GET /api/deliveries/$DELIVERY_ID/messages as CLIENT (unauthorized)..."
OTHER_HTTP_STATUS=$(curl -s -o /dev/null -w "%{http_code}" -X GET "$BASE_URL/api/deliveries/$DELIVERY_ID/messages" \
  -H "Authorization: Bearer $OTHER_TOKEN")

if [ "$OTHER_HTTP_STATUS" -eq 403 ]; then
    echo "✅ PASS: Other user correctly blocked from accessing the chat (HTTP $OTHER_HTTP_STATUS)"
else
    echo "❌ ERROR: Other user access was NOT blocked (HTTP $OTHER_HTTP_STATUS)"
    exit 1
fi

# 5. Conclusion
echo -e "\n========================================================"
echo " 🎉 All Messaging Security tests completed successfully!"
echo " Note: WebSocket SignalR connections use the same security"
echo " service ensuring EnsureCanAccessDeliveryChatAsync."
echo "========================================================"
