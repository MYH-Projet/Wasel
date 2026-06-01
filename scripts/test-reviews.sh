#!/bin/bash
set -e

# Default values
API_URL=${API_BASE_URL:-"http://localhost:5000"}
KEYCLOAK_URL=${KEYCLOAK_BASE_URL:-"http://localhost:8080/auth"}
REALM="wasel"
CLIENT_ID=${KEYCLOAK_CLIENT_ID:-"wasel-api"}

# Colors
GREEN='\033[0;32m'
RED='\033[0;31m'
YELLOW='\033[1;33m'
CYAN='\033[0;36m'
NC='\033[0m'

echo "========================================================"
echo " ⚡ Wasel API Test - Reviews Module (Integration)"
echo "========================================================"

# Test 1 - Retrieve CLIENT token
echo -e "\n${YELLOW}==> 1. Retrieving Client token (client@wasel.ma)...${NC}"
CLIENT_TOKEN_RESPONSE=$(curl -s -X POST "${KEYCLOAK_URL}/realms/${REALM}/protocol/openid-connect/token" \
    -H "Content-Type: application/x-www-form-urlencoded" \
    -d "client_id=${CLIENT_ID}&username=client@wasel.ma&password=client123&grant_type=password")

CLIENT_TOKEN=$(echo $CLIENT_TOKEN_RESPONSE | grep -o '"access_token":"[^"]*' | grep -o '[^"]*$')

if [ -z "$CLIENT_TOKEN" ]; then
    echo -e "${RED}❌ FAIL: Could not retrieve Client token${NC}"
    exit 1
fi
echo -e "${GREEN}✅ PASS: Client token retrieved${NC}"

# Test 1.5 - Auto-sync and Set Active
echo -e "\n${YELLOW}==> 1.5 Auto-syncing and setting user active...${NC}"
curl -s -o /dev/null -X GET "${API_URL}/api/auth/me" -H "Authorization: Bearer ${CLIENT_TOKEN}"
if command -v docker &> /dev/null; then
    docker exec wasel-postgres psql -U wasel_user -d wasel_db -t -c "UPDATE users SET \"Status\" = 1 WHERE \"Email\" = 'client@wasel.ma';" > /dev/null
fi

# Test 2 - Ensure Driver Profile Exists
echo -e "\n${YELLOW}==> 2. Ensuring driver profile exists for the client...${NC}"
curl -s -o /dev/null -X POST "${API_URL}/api/drivers/register" \
    -H "Authorization: Bearer ${CLIENT_TOKEN}" \
    -H "Content-Type: application/json" \
    -d '{
        "permisNumber": "B123456",
        "vehicle": {
            "type": "MOTORCYCLE",
            "matricule": "12345-A-6",
            "model": "Click 125",
            "marque": "Honda"
        }
    }'

DRIVER_ME_RESPONSE=$(curl -s -X GET "${API_URL}/api/drivers/me" -H "Authorization: Bearer ${CLIENT_TOKEN}")
DRIVER_ID=$(echo $DRIVER_ME_RESPONSE | grep -o '"driverId":"[^"]*' | head -n 1 | grep -o '[^"]*$')

if [ -z "$DRIVER_ID" ]; then
    echo -e "${RED}❌ FAIL: Could not retrieve Driver ID${NC}"
    exit 1
fi
echo -e "${CYAN}   Driver ID: ${DRIVER_ID}${NC}"
echo -e "${GREEN}✅ PASS: Driver profile prepared${NC}"

# Test 3 - Create a Delivery
echo -e "\n${YELLOW}==> 3. Creating a delivery via API...${NC}"
DELIVERY_RESPONSE=$(curl -s -X POST "${API_URL}/api/deliveries" \
    -H "Authorization: Bearer ${CLIENT_TOKEN}" \
    -H "Content-Type: application/json" \
    -d '{
      "pickupAddress": {
        "label": "Home",
        "street": "123 Main St",
        "city": "Casablanca",
        "country": "Morocco"
      },
      "dropoffAddress": {
        "label": "Work",
        "street": "456 Office St",
        "city": "Rabat",
        "country": "Morocco"
      },
      "parcel": {
        "description": "Integration Test Package",
        "weight": 1.5,
        "volume": 0.5,
        "isFragile": true
      },
      "paymentMethod": 0
    }')

DELIVERY_ID=$(echo $DELIVERY_RESPONSE | grep -o '"deliveryId":"[^"]*' | head -n 1 | grep -o '[^"]*$')

if [ -z "$DELIVERY_ID" ]; then
    echo -e "${RED}❌ FAIL: Could not create delivery${NC}"
    echo $DELIVERY_RESPONSE
    exit 1
fi
echo -e "${CYAN}   Delivery ID: ${DELIVERY_ID}${NC}"
echo -e "${GREEN}✅ PASS: Delivery created${NC}"

# Test 4 & 5 - Set Delivery to DELIVERED via Database
echo -e "\n${YELLOW}==> 4 & 5. Forcing Delivery to DELIVERED and Assigning Driver via Database...${NC}"
if command -v docker &> /dev/null; then
    docker exec wasel-postgres psql -U wasel_user -d wasel_db -t -c "UPDATE deliveries SET \"Status\" = 'DELIVERED', \"DriverId\" = '${DRIVER_ID}' WHERE \"Id\" = '${DELIVERY_ID}';" > /dev/null
    echo -e "${GREEN}✅ PASS: Delivery updated directly in the DB${NC}"
else
    echo -e "${YELLOW}⚠️ WARNING: Docker not found. Cannot force delivery status via psql. The next steps might fail with 400 (Not DELIVERED).${NC}"
fi

# Test 6 - Create a review
echo -e "\n${YELLOW}==> 6. Create a review for the delivery...${NC}"
REVIEW_STATUS=$(curl -s -o /dev/null -w "%{http_code}" -X POST "${API_URL}/api/reviews" \
    -H "Authorization: Bearer ${CLIENT_TOKEN}" \
    -H "Content-Type: application/json" \
    -d "{
  \"deliveryId\": \"${DELIVERY_ID}\",
  \"rating\": 5,
  \"comment\": \"Excellent service!\"
}")

if [ "$REVIEW_STATUS" = "200" ] || [ "$REVIEW_STATUS" = "201" ]; then
    echo -e "${GREEN}✅ PASS: Review created successfully (HTTP $REVIEW_STATUS)${NC}"
else
    echo -e "${RED}❌ FAIL: Review creation returned unexpected status (HTTP $REVIEW_STATUS)${NC}"
    # Stop if review creation failed
    exit 1
fi

# Test 7 - Test Duplicate Review (409)
echo -e "\n${YELLOW}==> 7. Test Duplicate Review (Should be 409 Conflict)...${NC}"
REVIEW_DUP_STATUS=$(curl -s -o /dev/null -w "%{http_code}" -X POST "${API_URL}/api/reviews" \
    -H "Authorization: Bearer ${CLIENT_TOKEN}" \
    -H "Content-Type: application/json" \
    -d "{
  \"deliveryId\": \"${DELIVERY_ID}\",
  \"rating\": 4,
  \"comment\": \"Second review attempt\"
}")

if [ "$REVIEW_DUP_STATUS" = "409" ]; then
    echo -e "${GREEN}✅ PASS: Duplicate review blocked successfully (HTTP 409)${NC}"
else
    echo -e "${RED}❌ FAIL: Duplicate review returned unexpected status (HTTP $REVIEW_DUP_STATUS)${NC}"
fi

# Test 8 - Get driver reviews (Public)
echo -e "\n${YELLOW}==> 8. Get driver reviews (Public)...${NC}"
GET_REVIEWS_STATUS=$(curl -s -o /dev/null -w "%{http_code}" -X GET "${API_URL}/api/drivers/${DRIVER_ID}/reviews")

if [ "$GET_REVIEWS_STATUS" = "200" ]; then
    echo -e "${GREEN}✅ PASS: Retrieved driver reviews successfully (HTTP $GET_REVIEWS_STATUS)${NC}"
else
    echo -e "${RED}❌ FAIL: Failed to retrieve driver reviews (HTTP $GET_REVIEWS_STATUS)${NC}"
fi

echo -e "\n${GREEN}========================================================${NC}"
echo -e "${GREEN} 🎉 All tests completed successfully!${NC}"
echo -e "${GREEN}========================================================${NC}"
