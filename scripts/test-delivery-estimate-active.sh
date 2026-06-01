#!/bin/bash
set -e

echo "========================================="
echo "  Testing Delivery Estimate and Active API "
echo "========================================="

API_BASE_URL="http://localhost:5000/api"
KEYCLOAK_URL="http://localhost:8080/auth"
REALM="wasel"
CLIENT_ID="wasel-api"

USER_EMAIL="client@wasel.ma"
USER_PASS="client123"

# 1. Get Token
echo -n "1. Authenticating as client... "
TOKEN_RESPONSE=$(curl -s -X POST "$KEYCLOAK_URL/realms/$REALM/protocol/openid-connect/token" \
  -H "Content-Type: application/x-www-form-urlencoded" \
  -d "client_id=$CLIENT_ID" \
  -d "username=$USER_EMAIL" \
  -d "password=$USER_PASS" \
  -d "grant_type=password")

TOKEN=$(echo $TOKEN_RESPONSE | grep -oP '"access_token":"\K[^"]+')

if [ -z "$TOKEN" ]; then
  echo "FAILED"
  exit 1
fi
echo "OK"

# 2. Test Estimate
echo -n "2. Testing GET /api/deliveries/estimate... "
ESTIMATE_RES=$(curl -s -w "%{http_code}" -o /tmp/est_body.json -H "Authorization: Bearer $TOKEN" "$API_BASE_URL/deliveries/estimate?pickupLat=33.5&pickupLng=-7.5&dropoffLat=34.0&dropoffLng=-6.8&weight=5&isFragile=true")

if [ "$ESTIMATE_RES" -eq 200 ]; then
  echo "OK"
  cat /tmp/est_body.json
  echo ""
else
  echo "FAILED (HTTP $ESTIMATE_RES)"
  exit 1
fi

# 3. Test Estimate Error
echo -n "3. Testing GET /api/deliveries/estimate with invalid weight... "
ESTIMATE_ERR=$(curl -s -w "%{http_code}" -o /tmp/esterr_body.json -H "Authorization: Bearer $TOKEN" "$API_BASE_URL/deliveries/estimate?pickupLat=33.5&pickupLng=-7.5&dropoffLat=34.0&dropoffLng=-6.8&weight=0&isFragile=true")

if [ "$ESTIMATE_ERR" -eq 400 ]; then
  echo "OK"
else
  echo "FAILED (Expected 400, got $ESTIMATE_ERR)"
  exit 1
fi

# 4. Test My Active
echo -n "4. Testing GET /api/deliveries/my/active... "
ACTIVE_RES=$(curl -s -w "%{http_code}" -o /tmp/act_body.json -H "Authorization: Bearer $TOKEN" "$API_BASE_URL/deliveries/my/active")

if [ "$ACTIVE_RES" -eq 200 ]; then
  echo "OK"
  cat /tmp/act_body.json
  echo ""
else
  echo "FAILED (HTTP $ACTIVE_RES)"
  exit 1
fi

echo "========================================="
echo "  All tests completed successfully.     "
echo "========================================="
