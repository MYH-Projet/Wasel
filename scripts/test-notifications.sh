#!/bin/bash

# Configuration
API_URL="http://localhost:5000"
KEYCLOAK_URL="http://localhost:8080/auth"
REALM="wasel"
CLIENT_ID="wasel-api"
CLIENT_USERNAME="client@wasel.ma"
CLIENT_PASSWORD="client123"

echo "========================================================"
echo " ⚡ Wasel API Test - Notifications Module"
echo "========================================================"

# 1. Retrieve token
echo -e "\n==> Test 1: Retrieving Client token..."
TOKEN_RESPONSE=$(curl -s -X POST "$KEYCLOAK_URL/realms/$REALM/protocol/openid-connect/token" \
  -H "Content-Type: application/x-www-form-urlencoded" \
  -d "client_id=$CLIENT_ID" \
  -d "username=$CLIENT_USERNAME" \
  -d "password=$CLIENT_PASSWORD" \
  -d "grant_type=password")

ACCESS_TOKEN=$(echo $TOKEN_RESPONSE | grep -o '"access_token":"[^"]*"' | cut -d'"' -f4)

if [ "$ACCESS_TOKEN" == "null" ] || [ -z "$ACCESS_TOKEN" ]; then
    echo "❌ FAIL: Could not retrieve token."
    exit 1
fi
echo "✅ PASS: Token retrieved."

# Determine the User ID directly from DB so we can inject notifications
# Note: we assume client1@wasel.ma corresponds to a User in DB. We will use the 'me' endpoint to get it if possible, but let's just trigger a notification via business logic.

# In this script we'll just check if GET /my returns 200, but there might be no notifications.
# Let's seed a notification into the DB directly for test purposes.

DB_CONTAINER="wasel-postgres"
DB_USER="wasel_user"
DB_NAME="wasel_db"

echo -e "\n==> Test 2: Checking current notifications (GET /api/notifications/my)..."
RES=$(curl -s -w "\n%{http_code}" -X GET "$API_URL/api/notifications/my?page=1&pageSize=10" \
  -H "Authorization: Bearer $ACCESS_TOKEN")

HTTP_STATUS=$(echo "$RES" | tail -n1)
BODY=$(echo "$RES" | head -n-1)

if [ "$HTTP_STATUS" == "200" ]; then
    echo "✅ PASS: GET /api/notifications/my returned 200."
    echo $BODY

else
    echo "❌ FAIL: Expected 200, got $HTTP_STATUS"
    exit 1
fi

echo -e "\n==> Test 3: Seeding a notification for client via SQL..."
USER_ID=$(docker exec -i $DB_CONTAINER psql -U $DB_USER -d $DB_NAME -t -c "SELECT \"Id\" FROM public.users WHERE \"Email\" = 'client@wasel.ma' LIMIT 1;" | tr -d ' ' | tr -d '\n' | tr -d '\r')

if [ -z "$USER_ID" ]; then
    echo "❌ FAIL: Could not find user in DB to seed notification."
    exit 1
fi

NOTIF_ID=$(uuidgen | tr '[:upper:]' '[:lower:]')
docker exec -i $DB_CONTAINER psql -U $DB_USER -d $DB_NAME -c "INSERT INTO public.notifications (\"Id\", \"UserId\", \"Type\", \"Title\", \"Body\", \"Status\", \"CreatedAt\", \"UpdatedAt\") VALUES ('$NOTIF_ID', '$USER_ID', 'DELIVERY_ASSIGNED', 'Livreur assigné', 'Test', 'UNREAD', NOW(), NOW());"

echo "✅ PASS: Seeded notification $NOTIF_ID"

echo -e "\n==> Test 4: Verifying the seeded notification appears..."
RES=$(curl -s -X GET "$API_URL/api/notifications/my?page=1&pageSize=10" -H "Authorization: Bearer $ACCESS_TOKEN")
UNREAD_COUNT=$(echo $RES | grep -o '"unreadCount":[0-9]*' | cut -d':' -f2)
if [ "$UNREAD_COUNT" -ge "1" ]; then
    echo "✅ PASS: Notification appears. UnreadCount=$UNREAD_COUNT"
else
    echo "❌ FAIL: Expected unread count >= 1"
    exit 1
fi

echo -e "\n==> Test 5: Marking notification as read (PATCH /api/notifications/{id}/read)..."
RES=$(curl -s -w "\n%{http_code}" -X PATCH "$API_URL/api/notifications/$NOTIF_ID/read" -H "Authorization: Bearer $ACCESS_TOKEN")
HTTP_STATUS=$(echo "$RES" | tail -n1)

if [ "$HTTP_STATUS" == "200" ]; then
    echo "✅ PASS: Marked as read (HTTP 200)."
else
    echo "❌ FAIL: Expected 200, got $HTTP_STATUS"
    exit 1
fi

echo -e "\n==> Test 6: Seeding multiple notifications for read-all test..."
docker exec -i $DB_CONTAINER psql -U $DB_USER -d $DB_NAME -c "INSERT INTO public.notifications (\"Id\", \"UserId\", \"Type\", \"Title\", \"Body\", \"Status\", \"CreatedAt\", \"UpdatedAt\") VALUES (gen_random_uuid(), '$USER_ID', 'NEW_MESSAGE', 'Msg 1', 'Test', 'UNREAD', NOW(), NOW()), (gen_random_uuid(), '$USER_ID', 'NEW_MESSAGE', 'Msg 2', 'Test', 'UNREAD', NOW(), NOW());"

echo -e "\n==> Test 7: Marking all as read (PATCH /api/notifications/read-all)..."
RES=$(curl -s -w "\n%{http_code}" -X PATCH "$API_URL/api/notifications/read-all" -H "Authorization: Bearer $ACCESS_TOKEN")
HTTP_STATUS=$(echo "$RES" | tail -n1)
UPDATED=$(echo "$RES" | head -n-1 | grep -o '"updatedCount":[0-9]*' | cut -d':' -f2)

if [ "$HTTP_STATUS" == "200" ]; then
    echo "✅ PASS: Marked all as read. UpdatedCount=$UPDATED"
else
    echo "❌ FAIL: Expected 200, got $HTTP_STATUS"
    exit 1
fi

echo -e "\n==> Test 8: Verifying unread count is 0..."
RES=$(curl -s -X GET "$API_URL/api/notifications/my?page=1&pageSize=10" -H "Authorization: Bearer $ACCESS_TOKEN")
UNREAD_COUNT=$(echo $RES | grep -o '"unreadCount":[0-9]*' | cut -d':' -f2)
if [ "$UNREAD_COUNT" == "0" ]; then
    echo "✅ PASS: Unread count is correctly 0."
else
    echo "❌ FAIL: Expected 0, got $UNREAD_COUNT"
    exit 1
fi

echo "========================================================"
echo " ✅ All Notifications module tests passed!"
echo "========================================================"
