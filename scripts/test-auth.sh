#!/bin/bash

# Configuration
API_BASE_URL=${API_BASE_URL:-"http://localhost:5000"}
KEYCLOAK_URL=${KEYCLOAK_URL:-"http://localhost:8080/auth"}
REALM=${REALM:-"wasel"}
CLIENT_ID=${CLIENT_ID:-"wasel-api"}
ADMIN_USERNAME=${ADMIN_USERNAME:-"admin@wasel.ma"}
ADMIN_PASSWORD=${ADMIN_PASSWORD:-"admin123"}
CLIENT_USERNAME=${CLIENT_USERNAME:-"client@wasel.ma"}
CLIENT_PASSWORD=${CLIENT_PASSWORD:-"client123"}

# Summary variables
TESTS_PASSED=0
TESTS_FAILED=0
declare -a SUMMARY

# Functions
print_step() {
    echo -e "\n========================================================"
    echo -e " ⚡ $1"
    echo -e "========================================================"
}

pass() {
    echo -e "✅ PASS: $1"
    SUMMARY+=("✅ PASS: $1")
    ((TESTS_PASSED++))
}

fail() {
    echo -e "❌ FAIL: $1"
    SUMMARY+=("❌ FAIL: $1")
    ((TESTS_FAILED++))
    if [ "$2" == "CRITICAL" ]; then
        echo -e "\n🚨 CRITICAL ERROR! Stopping tests."
        print_summary
        exit 1
    fi
}

warn() {
    echo -e "⚠️ WARN: $1"
    SUMMARY+=("⚠️ WARN: $1")
}

assert_status() {
    local expected=$1
    local actual=$2
    local name=$3
    if [ "$expected" == "$actual" ]; then
        pass "$name (Status $actual)"
    else
        fail "$name (Expected $expected, got $actual)"
    fi
}

get_token() {
    local username=$1
    local password=$2
    local response=$(curl -s --location --request POST "$KEYCLOAK_URL/realms/$REALM/protocol/openid-connect/token" \
        --header "Content-Type: application/x-www-form-urlencoded" \
        --data-urlencode "client_id=$CLIENT_ID" \
        --data-urlencode "username=$username" \
        --data-urlencode "password=$password" \
        --data-urlencode "grant_type=password")

    # Extract access_token using grep and sed (cross-platform, no jq needed)
    local token=$(echo "$response" | grep -o '"access_token":"[^"]*' | sed 's/"access_token":"//')
    echo "$token"
}

print_summary() {
    echo -e "\n========================================================"
    echo -e " 📊 Auth Test Summary"
    echo -e "========================================================"
    echo -e " API_BASE_URL:  $API_BASE_URL"
    echo -e " KEYCLOAK_URL:  $KEYCLOAK_URL"
    echo -e "--------------------------------------------------------"
    for item in "${SUMMARY[@]}"; do
        echo -e " $item"
    done
    echo -e "--------------------------------------------------------"
    echo -e " Total Passed: $TESTS_PASSED"
    echo -e " Total Failed: $TESTS_FAILED"
    echo -e "========================================================"
}

# --- PRE-CHECKS ---
print_step "Pre-Checks"

# Check API
API_STATUS=$(curl -s -o /dev/null -w "%{http_code}" "$API_BASE_URL/api/health")
if [ "$API_STATUS" != "200" ]; then
    fail "API is not accessible at $API_BASE_URL/api/health (Status $API_STATUS)" "CRITICAL"
else
    echo "API is accessible."
fi

# Check Keycloak
KC_STATUS=$(curl -s -o /dev/null -w "%{http_code}" "$KEYCLOAK_URL/realms/$REALM")
if [ "$KC_STATUS" != "200" ]; then
    echo "Realm $REALM not found. Please configure Keycloak manually using Infrastructure/Keycloak/KeycloakSetupGuide.md"
    fail "Keycloak realm '$REALM' is not accessible at $KEYCLOAK_URL (Status $KC_STATUS)" "CRITICAL"
else
    echo "Keycloak realm '$REALM' is accessible."
fi

# --- TESTS ---

# Test 1 - API health
print_step "Test 1 — API health"
STATUS=$(curl -s -o /dev/null -w "%{http_code}" "$API_BASE_URL/api/health")
assert_status "200" "$STATUS" "API health endpoint"

# Test 2 - Admin users without token
print_step "Test 2 — Admin users sans token"
STATUS=$(curl -s -o /dev/null -w "%{http_code}" "$API_BASE_URL/api/admin/users")
assert_status "401" "$STATUS" "Admin endpoint without token"

# Test 3 - Admin token retrieval
print_step "Test 3 — Récupération token ADMIN"
ADMIN_TOKEN=$(get_token "$ADMIN_USERNAME" "$ADMIN_PASSWORD")
if [ -n "$ADMIN_TOKEN" ]; then
    pass "Admin token retrieved (${ADMIN_TOKEN:0:30}...)"
else
    fail "Admin token retrieval failed (Empty token)" "CRITICAL"
fi

# Test 4 - Auth me admin (AUTO-SYNC: user local is auto-created here, BEFORE any /api/auth/sync call)
print_step "Test 4 — /api/auth/me avec ADMIN (auto-sync)"
RESPONSE=$(curl -s -w "\n%{http_code}" -X GET "$API_BASE_URL/api/auth/me" -H "Authorization: Bearer $ADMIN_TOKEN")
STATUS=$(echo "$RESPONSE" | tail -1)
BODY=$(echo "$RESPONSE" | sed '$d')
assert_status "200" "$STATUS" "Auth me admin (auto-sync)"

# Verify that localUserId is present (proof the user was auto-created)
LOCAL_USER_ID=$(echo "$BODY" | grep -o '"localUserId":"[^"]*' | sed 's/"localUserId":"//')
if [ -n "$LOCAL_USER_ID" ] && [ "$LOCAL_USER_ID" != "null" ]; then
    pass "Auth me admin returns localUserId ($LOCAL_USER_ID) — auto-sync works"
else
    fail "Auth me admin missing localUserId — auto-sync did not create the local user"
fi

# Test 5 - Auth claims admin
print_step "Test 5 — /api/auth/claims avec ADMIN"
STATUS=$(curl -s -o /dev/null -w "%{http_code}" -X GET "$API_BASE_URL/api/auth/claims" -H "Authorization: Bearer $ADMIN_TOKEN")
if [ "$STATUS" == "200" ]; then
    pass "Auth claims admin (Status 200)"
else
    warn "Auth claims admin returned $STATUS (Endpoint might be disabled in non-dev)"
fi

# Test 6 - Auth sync admin (backward compatibility — still works)
print_step "Test 6 — /api/auth/sync avec ADMIN (backward compat)"
STATUS=$(curl -s -o /dev/null -w "%{http_code}" -X POST "$API_BASE_URL/api/auth/sync" -H "Authorization: Bearer $ADMIN_TOKEN")
assert_status "200" "$STATUS" "Auth sync admin"

# Test 7 - Admin users admin
print_step "Test 7 — /api/admin/users avec ADMIN"
STATUS=$(curl -s -o /dev/null -w "%{http_code}" -X GET "$API_BASE_URL/api/admin/users" -H "Authorization: Bearer $ADMIN_TOKEN")
assert_status "200" "$STATUS" "Admin users admin"

# Test 8 - Client token retrieval
print_step "Test 8 — Récupération token CLIENT"
CLIENT_TOKEN=$(get_token "$CLIENT_USERNAME" "$CLIENT_PASSWORD")
if [ -n "$CLIENT_TOKEN" ]; then
    pass "Client token retrieved (${CLIENT_TOKEN:0:30}...)"
else
    fail "Client token retrieval failed (Empty token)"
fi

# Test 9 - Auth me client (AUTO-SYNC: client user local is auto-created here too)
print_step "Test 9 — /api/auth/me avec CLIENT (auto-sync)"
RESPONSE=$(curl -s -w "\n%{http_code}" -X GET "$API_BASE_URL/api/auth/me" -H "Authorization: Bearer $CLIENT_TOKEN")
STATUS=$(echo "$RESPONSE" | tail -1)
BODY=$(echo "$RESPONSE" | sed '$d')
assert_status "200" "$STATUS" "Auth me client (auto-sync)"

# Verify client localUserId
CLIENT_LOCAL_ID=$(echo "$BODY" | grep -o '"localUserId":"[^"]*' | sed 's/"localUserId":"//')
if [ -n "$CLIENT_LOCAL_ID" ] && [ "$CLIENT_LOCAL_ID" != "null" ]; then
    pass "Auth me client returns localUserId ($CLIENT_LOCAL_ID) — auto-sync works"
else
    fail "Auth me client missing localUserId — auto-sync did not create the local user"
fi

# Test 10 - Admin users client
print_step "Test 10 — /api/admin/users avec CLIENT"
STATUS=$(curl -s -o /dev/null -w "%{http_code}" -X GET "$API_BASE_URL/api/admin/users" -H "Authorization: Bearer $CLIENT_TOKEN")
assert_status "403" "$STATUS" "Admin users client forbidden"

# Test 11 - Invalid token
print_step "Test 11 — Token invalide"
STATUS=$(curl -s -o /dev/null -w "%{http_code}" -X GET "$API_BASE_URL/api/auth/me" -H "Authorization: Bearer invalid_token_123")
assert_status "401" "$STATUS" "Invalid token"

# Test 12 - Header without Bearer
print_step "Test 12 — Header sans Bearer"
STATUS=$(curl -s -o /dev/null -w "%{http_code}" -X GET "$API_BASE_URL/api/auth/me" -H "Authorization: $ADMIN_TOKEN")
assert_status "401" "$STATUS" "Header without Bearer"

# Test 14 - Admin change user status
print_step "Test 14 — Admin change user status"
# Get the first user ID
FIRST_USER_ID=$(curl -s -X GET "$API_BASE_URL/api/admin/users" -H "Authorization: Bearer $ADMIN_TOKEN" | grep -o '"id":"[^"]*' | head -1 | sed 's/"id":"//')
if [ -n "$FIRST_USER_ID" ]; then
    STATUS=$(curl -s -o /dev/null -w "%{http_code}" -X PATCH "$API_BASE_URL/api/admin/users/$FIRST_USER_ID/status" \
        -H "Authorization: Bearer $ADMIN_TOKEN" \
        -H "Content-Type: application/json" \
        -d '{"status":1}')
    assert_status "200" "$STATUS" "Admin change user status"
else
    warn "No users found to test status change"
fi

# Test 15 - Update Admin Profile (no /api/auth/sync needed beforehand — auto-sync handles it)
print_step "Test 15 — Update Admin Profile (auto-sync)"
STATUS=$(curl -s -o /dev/null -w "%{http_code}" -X PATCH "$API_BASE_URL/api/auth/me/profile" \
    -H "Authorization: Bearer $ADMIN_TOKEN" \
    -H "Content-Type: application/json" \
    -d '{"cin":"TEST1234", "phone":"+212600000000"}')
assert_status "200" "$STATUS" "Update Admin Profile"

# Test 13 - Verify API logs for critical errors
print_step "Test 13 — Vérification logs API"
echo "Fetching recent API logs..."
LOGS=$(docker compose logs --tail=100 wasel-api 2>&1)
CRITICAL_ERRORS=$(echo "$LOGS" | grep -iE 'Issuer validation failed|Signature validation failed|Audience validation failed|Database update failed|Unhandled exception')

if [ -n "$CRITICAL_ERRORS" ]; then
    warn "Found critical errors in recent API logs:\n$CRITICAL_ERRORS"
else
    pass "No critical auth errors found in recent logs"
fi

print_summary
