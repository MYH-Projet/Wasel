#!/bin/bash

# Configuration
API_BASE_URL=${API_BASE_URL:-"http://localhost:5000"}
KEYCLOAK_URL=${KEYCLOAK_URL:-"http://localhost:8080"}
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

# Test 4 - Auth me admin
print_step "Test 4 — /api/auth/me avec ADMIN"
STATUS=$(curl -s -o /dev/null -w "%{http_code}" -X GET "$API_BASE_URL/api/auth/me" -H "Authorization: Bearer $ADMIN_TOKEN")
assert_status "200" "$STATUS" "Auth me admin"

# Test 5 - Auth claims admin
print_step "Test 5 — /api/auth/claims avec ADMIN"
STATUS=$(curl -s -o /dev/null -w "%{http_code}" -X GET "$API_BASE_URL/api/auth/claims" -H "Authorization: Bearer $ADMIN_TOKEN")
if [ "$STATUS" == "200" ]; then
    pass "Auth claims admin (Status 200)"
else
    warn "Auth claims admin returned $STATUS (Endpoint might be disabled in non-dev)"
fi

# Test 6 - Auth sync admin
print_step "Test 6 — /api/auth/sync avec ADMIN"
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

# Test 9 - Auth me client
print_step "Test 9 — /api/auth/me avec CLIENT"
STATUS=$(curl -s -o /dev/null -w "%{http_code}" -X GET "$API_BASE_URL/api/auth/me" -H "Authorization: Bearer $CLIENT_TOKEN")
assert_status "200" "$STATUS" "Auth me client"

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
