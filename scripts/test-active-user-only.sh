#!/bin/bash
set -e

# Default values
API_URL=${API_BASE_URL:-"http://localhost:5000"}
KEYCLOAK_URL=${KEYCLOAK_BASE_URL:-"http://localhost:8080/auth/realms/wasel/protocol/openid-connect/token"}
CLIENT_ID=${KEYCLOAK_CLIENT_ID:-"wasel-api"}

# Colors
GREEN='\033[0;32m'
RED='\033[0;31m'
YELLOW='\033[1;33m'
NC='\033[0m'

echo "========================================================"
echo " ⚡ Wasel API Test - ActiveUserOnly Policy"
echo "========================================================"

# Trap to ensure cleanup (reset user to Active)
cleanup() {
    echo -e "\n${YELLOW}==> Cleanup: Resetting user to Active...${NC}"
    if command -v docker &> /dev/null; then
        docker exec wasel-postgres psql -U wasel_user -d wasel_db -t -c "UPDATE users SET \"Status\" = 1 WHERE \"Email\" = 'client@wasel.ma';" > /dev/null
    fi
    echo -e "${GREEN}✅ Cleanup complete.${NC}"
}
trap cleanup EXIT

# 1. Retrieve CLIENT token
echo -e "\n${YELLOW}==> 1. Retrieving Client token...${NC}"
CLIENT_TOKEN=$(curl -s -X POST "${KEYCLOAK_URL}" \
    -H "Content-Type: application/x-www-form-urlencoded" \
    -d "client_id=${CLIENT_ID}&username=client@wasel.ma&password=client123&grant_type=password" | grep -o '"access_token":"[^"]*' | grep -o '[^"]*$')

if [ -z "$CLIENT_TOKEN" ]; then
    echo -e "${RED}❌ FAIL: Could not retrieve Client token${NC}"
    exit 1
fi
echo -e "${GREEN}✅ PASS: Client token retrieved${NC}"

# 2. Trigger auto-sync and ensure user exists
echo -e "\n${YELLOW}==> 2. Syncing user /api/auth/me...${NC}"
curl -s -o /dev/null -X GET "${API_URL}/api/auth/me" -H "Authorization: Bearer ${CLIENT_TOKEN}"

# 3. Block user
echo -e "\n${YELLOW}==> 3. Setting user status to Blocked (Status = 3)...${NC}"
if command -v docker &> /dev/null; then
    docker exec wasel-postgres psql -U wasel_user -d wasel_db -t -c "UPDATE users SET \"Status\" = 3 WHERE \"Email\" = 'client@wasel.ma';" > /dev/null
else
    echo -e "${RED}❌ FAIL: Docker is required for this test${NC}"
    exit 1
fi

# 4. Test protected endpoint (GET /api/notifications/my)
echo -e "\n${YELLOW}==> 4. Testing protected endpoint /api/notifications/my...${NC}"
NOTIFICATIONS_HTTP_CODE=$(curl -s -o /dev/null -w "%{http_code}" -X GET "${API_URL}/api/notifications/my" \
    -H "Authorization: Bearer ${CLIENT_TOKEN}")

if [ "$NOTIFICATIONS_HTTP_CODE" -eq 403 ]; then
    echo -e "${GREEN}✅ PASS: Blocked user access denied (HTTP 403)${NC}"
else
    echo -e "${RED}❌ FAIL: Blocked user accessed protected endpoint (HTTP ${NOTIFICATIONS_HTTP_CODE})${NC}"
    exit 1
fi

# 5. Test unprotected endpoint (/api/auth/me)
echo -e "\n${YELLOW}==> 5. Testing unprotected endpoint /api/auth/me...${NC}"
AUTH_ME_HTTP_CODE=$(curl -s -o /dev/null -w "%{http_code}" -X GET "${API_URL}/api/auth/me" \
    -H "Authorization: Bearer ${CLIENT_TOKEN}")

if [ "$AUTH_ME_HTTP_CODE" -eq 200 ]; then
    echo -e "${GREEN}✅ PASS: Blocked user access allowed to /api/auth/me (HTTP 200)${NC}"
else
    echo -e "${RED}❌ FAIL: Blocked user access denied to /api/auth/me (HTTP ${AUTH_ME_HTTP_CODE})${NC}"
    exit 1
fi

# 6. Set user Active
echo -e "\n${YELLOW}==> 6. Setting user status to Active (Status = 1)...${NC}"
docker exec wasel-postgres psql -U wasel_user -d wasel_db -t -c "UPDATE users SET \"Status\" = 1 WHERE \"Email\" = 'client@wasel.ma';" > /dev/null

# 7. Test protected endpoint again
echo -e "\n${YELLOW}==> 7. Testing protected endpoint /api/notifications/my again...${NC}"
NOTIFICATIONS_HTTP_CODE_2=$(curl -s -o /dev/null -w "%{http_code}" -X GET "${API_URL}/api/notifications/my" \
    -H "Authorization: Bearer ${CLIENT_TOKEN}")

if [ "$NOTIFICATIONS_HTTP_CODE_2" -eq 200 ]; then
    echo -e "${GREEN}✅ PASS: Active user accessed protected endpoint (HTTP 200)${NC}"
else
    echo -e "${RED}❌ FAIL: Active user access denied (HTTP ${NOTIFICATIONS_HTTP_CODE_2})${NC}"
    exit 1
fi

echo -e "\n${GREEN}========================================================${NC}"
echo -e "${GREEN} 🎉 All ActiveUserOnly tests completed successfully!${NC}"
echo -e "${GREEN}========================================================${NC}"
