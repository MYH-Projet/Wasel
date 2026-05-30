#!/bin/bash
# ══════════════════════════════════════════════════════════════
# Wasel — Files / MinIO Integration Test Script
# ══════════════════════════════════════════════════════════════
#
# Usage:
#   bash scripts/test-files-minio.sh
#
# Optional environment variables:
#   API_BASE_URL   (default: http://localhost:5000)
#   KEYCLOAK_URL   (default: http://localhost:8080/auth)
#   ADMIN_USER     (default: admin@wasel.ma)
#   ADMIN_PASS     (default: admin123)
#   CLIENT_USER    (default: client@wasel.ma)
#   CLIENT_PASS    (default: client123)
#   CLIENT_ID      (default: wasel-api)
#   REALM          (default: wasel)
# ══════════════════════════════════════════════════════════════

set -uo pipefail

# ── Configuration ─────────────────────────────────────────────
API_BASE_URL="${API_BASE_URL:-http://localhost:5000}"
KEYCLOAK_URL="${KEYCLOAK_URL:-http://localhost:8080/auth}"
ADMIN_USER="${ADMIN_USER:-admin@wasel.ma}"
ADMIN_PASS="${ADMIN_PASS:-admin123}"
CLIENT_USER="${CLIENT_USER:-client@wasel.ma}"
CLIENT_PASS="${CLIENT_PASS:-client123}"
CLIENT_ID="${CLIENT_ID:-wasel-api}"
REALM="${REALM:-wasel}"

# ── Counters ──────────────────────────────────────────────────
TESTS_PASSED=0
TESTS_FAILED=0
TESTS_SKIPPED=0
declare -a SUMMARY

# ── JSON Parser Selection ────────────────────────────────────
JSON_PARSER=""
if command -v jq &>/dev/null; then
    JSON_PARSER="jq"
else
    JSON_PARSER="grep"
fi

json_get() {
    local json="$1"
    local field="$2"
    if [ "$JSON_PARSER" = "jq" ]; then
        echo "$json" | jq -r ".$field // empty" 2>/dev/null
    else
        # grep/sed fallback — works on Git Bash, Linux, macOS
        # Try string value first ("field":"value"), then numeric (field:123)
        local result
        result=$(echo "$json" | grep -o "\"$field\":\"[^\"]*" | sed "s/\"$field\":\"//")
        if [ -z "$result" ]; then
            result=$(echo "$json" | grep -o "\"$field\":[0-9]*" | sed "s/\"$field\"://")
        fi
        echo "$result"
    fi
}

# ── Helper Functions ──────────────────────────────────────────
print_section() {
    echo ""
    echo "========================================================"
    echo " ⚡ $1"
    echo "========================================================"
}

pass() {
    echo "  ✅ PASS: $1"
    SUMMARY+=("✅ PASS: $1")
    ((TESTS_PASSED++))
}

fail() {
    echo "  ❌ FAIL: $1"
    SUMMARY+=("❌ FAIL: $1")
    ((TESTS_FAILED++))
}

skip() {
    echo "  ⏭️  SKIP: $1"
    SUMMARY+=("⏭️  SKIP: $1")
    ((TESTS_SKIPPED++))
}

warn() {
    echo "  ⚠️  WARN: $1"
}

assert_status() {
    local expected="$1"
    local actual="$2"
    local name="$3"
    if [ "$expected" = "$actual" ]; then
        pass "$name (HTTP $actual)"
    else
        fail "$name (expected $expected, got $actual)"
    fi
}

get_token() {
    local username="$1"
    local password="$2"
    local response
    response=$(curl -s --max-time 10 -X POST \
        "$KEYCLOAK_URL/realms/$REALM/protocol/openid-connect/token" \
        -H "Content-Type: application/x-www-form-urlencoded" \
        -d "client_id=$CLIENT_ID" \
        -d "username=$username" \
        -d "password=$password" \
        -d "grant_type=password" 2>/dev/null)
    json_get "$response" "access_token"
}

print_summary() {
    echo ""
    echo "========================================================"
    echo " 📊 Files / MinIO Integration Test Summary"
    echo "========================================================"
    echo " API_BASE_URL:  $API_BASE_URL"
    echo " KEYCLOAK_URL:  $KEYCLOAK_URL"
    echo "--------------------------------------------------------"
    for item in "${SUMMARY[@]}"; do
        echo "  $item"
    done
    echo "--------------------------------------------------------"
    echo "  Total Passed:  $TESTS_PASSED"
    echo "  Total Failed:  $TESTS_FAILED"
    echo "  Total Skipped: $TESTS_SKIPPED"
    echo "========================================================"
    if [ "$TESTS_FAILED" -gt 0 ]; then
        echo "  ❌ RESULT: SOME TESTS FAILED"
    else
        echo "  ✅ RESULT: ALL TESTS PASSED"
    fi
    echo "========================================================"
}

# ══════════════════════════════════════════════════════════════
# PRE-CHECKS
# ══════════════════════════════════════════════════════════════
print_section "Pre-Checks"

# Check API health
echo "  Checking API at $API_BASE_URL/api/health ..."
API_STATUS=$(curl -s -o /dev/null -w "%{http_code}" --max-time 10 "$API_BASE_URL/api/health" 2>/dev/null)
if [ "$API_STATUS" != "200" ]; then
    fail "API is not accessible at $API_BASE_URL/api/health (HTTP $API_STATUS)"
    echo ""
    echo "  🚨 CRITICAL: API is not running. Cannot continue."
    print_summary
    exit 1
else
    echo "  API is accessible."
fi

# Check Keycloak realm
echo "  Checking Keycloak realm at $KEYCLOAK_URL/realms/$REALM ..."
KC_STATUS=$(curl -s -o /dev/null -w "%{http_code}" --max-time 10 "$KEYCLOAK_URL/realms/$REALM" 2>/dev/null)
if [ "$KC_STATUS" != "200" ]; then
    fail "Keycloak realm '$REALM' is not accessible (HTTP $KC_STATUS)"
    echo ""
    echo "  🚨 CRITICAL: Keycloak is not running. Cannot continue."
    print_summary
    exit 1
else
    echo "  Keycloak realm '$REALM' is accessible."
fi

# Check MinIO container (optional — non-blocking)
echo "  Checking MinIO container ..."
if command -v docker &>/dev/null; then
    MINIO_STATUS=$(docker compose ps --format '{{.State}}' wasel-minio 2>/dev/null || echo "unknown")
    if echo "$MINIO_STATUS" | grep -qi "running"; then
        echo "  MinIO container is running."
    else
        warn "MinIO container status: $MINIO_STATUS (may not affect API-level tests if backend can reach MinIO)"
    fi
else
    warn "Docker CLI not available — cannot check MinIO container directly."
fi

echo "  JSON parser: $JSON_PARSER"

# ══════════════════════════════════════════════════════════════
# TEST 1 — Retrieve ADMIN token
# ══════════════════════════════════════════════════════════════
print_section "Test 1 — Retrieve ADMIN token"
ADMIN_TOKEN=$(get_token "$ADMIN_USER" "$ADMIN_PASS")
if [ -n "$ADMIN_TOKEN" ]; then
    pass "Admin token retrieved (${ADMIN_TOKEN:0:20}...)"
else
    fail "Admin token retrieval failed (empty token)"
    echo "  🚨 CRITICAL: Cannot continue without admin token."
    print_summary
    exit 1
fi

# Auto-sync admin so keycloakId is available for objectKey checks
curl -s -o /dev/null -X GET "$API_BASE_URL/api/auth/me" -H "Authorization: Bearer $ADMIN_TOKEN" 2>/dev/null

# ══════════════════════════════════════════════════════════════
# TEST 2 — Retrieve CLIENT token
# ══════════════════════════════════════════════════════════════
print_section "Test 2 — Retrieve CLIENT token"
CLIENT_TOKEN=$(get_token "$CLIENT_USER" "$CLIENT_PASS")
if [ -n "$CLIENT_TOKEN" ]; then
    pass "Client token retrieved (${CLIENT_TOKEN:0:20}...)"
else
    fail "Client token retrieval failed (empty token)"
    echo "  🚨 CRITICAL: Cannot continue without client token."
    print_summary
    exit 1
fi

# Auto-sync client so keycloakId is available for objectKey checks
curl -s -o /dev/null -X GET "$API_BASE_URL/api/auth/me" -H "Authorization: Bearer $CLIENT_TOKEN" 2>/dev/null

# ══════════════════════════════════════════════════════════════
# TEST 3 — POST /api/files/upload-url without token → 401
# ══════════════════════════════════════════════════════════════
print_section "Test 3 — POST /api/files/upload-url sans token"
STATUS=$(curl -s -o /dev/null -w "%{http_code}" --max-time 10 \
    -X POST "$API_BASE_URL/api/files/upload-url" \
    -H "Content-Type: application/json" \
    -d '{"fileName":"test.pdf","fileType":"pdf","context":"DOCUMENT"}' 2>/dev/null)
assert_status "401" "$STATUS" "upload-url without token"

# ══════════════════════════════════════════════════════════════
# TEST 4 — POST /api/files/upload-url PDF DOCUMENT (CLIENT)
# ══════════════════════════════════════════════════════════════
print_section "Test 4 — upload-url PDF DOCUMENT (CLIENT)"
RESPONSE=$(curl -s -w "\n%{http_code}" --max-time 10 \
    -X POST "$API_BASE_URL/api/files/upload-url" \
    -H "Authorization: Bearer $CLIENT_TOKEN" \
    -H "Content-Type: application/json" \
    -d '{"fileName":"permis.pdf","fileType":"pdf","context":"DOCUMENT"}' 2>/dev/null)
STATUS=$(echo "$RESPONSE" | tail -1)
BODY=$(echo "$RESPONSE" | sed '$d')

assert_status "200" "$STATUS" "upload-url PDF DOCUMENT"

if [ "$STATUS" = "200" ]; then
    UPLOAD_URL=$(json_get "$BODY" "uploadUrl")
    OBJECT_KEY=$(json_get "$BODY" "objectKey")
    EXPIRES=$(json_get "$BODY" "expiresInSeconds")

    if [ -n "$UPLOAD_URL" ]; then
        pass "uploadUrl is not empty"
    else
        fail "uploadUrl is empty"
    fi

    if [ -n "$OBJECT_KEY" ]; then
        pass "objectKey is not empty ($OBJECT_KEY)"
    else
        fail "objectKey is empty"
    fi

    if echo "$OBJECT_KEY" | grep -q "^documents/"; then
        pass "objectKey starts with documents/"
    else
        fail "objectKey does not start with documents/ ($OBJECT_KEY)"
    fi

    if echo "$OBJECT_KEY" | grep -q "\.pdf$"; then
        pass "objectKey ends with .pdf"
    else
        fail "objectKey does not end with .pdf ($OBJECT_KEY)"
    fi

    if [ "$EXPIRES" = "600" ]; then
        pass "expiresInSeconds = 600"
    else
        fail "expiresInSeconds expected 600, got $EXPIRES"
    fi

    # Save for later tests
    SAVED_OBJECT_KEY="$OBJECT_KEY"
    SAVED_UPLOAD_URL="$UPLOAD_URL"
else
    SAVED_OBJECT_KEY=""
    SAVED_UPLOAD_URL=""
fi

# ══════════════════════════════════════════════════════════════
# TEST 5 — POST /api/files/upload-url JPG PROFILE_PHOTO
# ══════════════════════════════════════════════════════════════
print_section "Test 5 — upload-url JPG PROFILE_PHOTO (CLIENT)"
RESPONSE=$(curl -s -w "\n%{http_code}" --max-time 10 \
    -X POST "$API_BASE_URL/api/files/upload-url" \
    -H "Authorization: Bearer $CLIENT_TOKEN" \
    -H "Content-Type: application/json" \
    -d '{"fileName":"photo.jpg","fileType":"jpg","context":"PROFILE_PHOTO"}' 2>/dev/null)
STATUS=$(echo "$RESPONSE" | tail -1)
BODY=$(echo "$RESPONSE" | sed '$d')

assert_status "200" "$STATUS" "upload-url JPG PROFILE_PHOTO"

if [ "$STATUS" = "200" ]; then
    OBJ=$(json_get "$BODY" "objectKey")
    EXP=$(json_get "$BODY" "expiresInSeconds")

    if echo "$OBJ" | grep -q "^profile-photos/"; then
        pass "objectKey starts with profile-photos/"
    else
        fail "objectKey does not start with profile-photos/ ($OBJ)"
    fi

    if [ "$EXP" = "600" ]; then
        pass "expiresInSeconds = 600"
    else
        fail "expiresInSeconds expected 600, got $EXP"
    fi
fi

# ══════════════════════════════════════════════════════════════
# TEST 6 — POST /api/files/upload-url PNG COMPLAINT_EVIDENCE
# ══════════════════════════════════════════════════════════════
print_section "Test 6 — upload-url PNG COMPLAINT_EVIDENCE (CLIENT)"
RESPONSE=$(curl -s -w "\n%{http_code}" --max-time 10 \
    -X POST "$API_BASE_URL/api/files/upload-url" \
    -H "Authorization: Bearer $CLIENT_TOKEN" \
    -H "Content-Type: application/json" \
    -d '{"fileName":"evidence.png","fileType":"png","context":"COMPLAINT_EVIDENCE"}' 2>/dev/null)
STATUS=$(echo "$RESPONSE" | tail -1)
BODY=$(echo "$RESPONSE" | sed '$d')

assert_status "200" "$STATUS" "upload-url PNG COMPLAINT_EVIDENCE"

if [ "$STATUS" = "200" ]; then
    OBJ=$(json_get "$BODY" "objectKey")
    if echo "$OBJ" | grep -q "^complaint-evidence/"; then
        pass "objectKey starts with complaint-evidence/"
    else
        fail "objectKey does not start with complaint-evidence/ ($OBJ)"
    fi
fi

# ══════════════════════════════════════════════════════════════
# TEST 7 — POST /api/files/upload-url JPG DELIVERY_PROOF
# ══════════════════════════════════════════════════════════════
print_section "Test 7 — upload-url JPG DELIVERY_PROOF (CLIENT)"
RESPONSE=$(curl -s -w "\n%{http_code}" --max-time 10 \
    -X POST "$API_BASE_URL/api/files/upload-url" \
    -H "Authorization: Bearer $CLIENT_TOKEN" \
    -H "Content-Type: application/json" \
    -d '{"fileName":"proof.jpg","fileType":"jpg","context":"DELIVERY_PROOF"}' 2>/dev/null)
STATUS=$(echo "$RESPONSE" | tail -1)
BODY=$(echo "$RESPONSE" | sed '$d')

assert_status "200" "$STATUS" "upload-url JPG DELIVERY_PROOF"

if [ "$STATUS" = "200" ]; then
    OBJ=$(json_get "$BODY" "objectKey")
    if echo "$OBJ" | grep -q "^delivery-proofs/"; then
        pass "objectKey starts with delivery-proofs/"
    else
        fail "objectKey does not start with delivery-proofs/ ($OBJ)"
    fi
fi

# ══════════════════════════════════════════════════════════════
# TEST 8 — POST /api/files/upload-url invalid fileType → 400
# ══════════════════════════════════════════════════════════════
print_section "Test 8 — upload-url invalid fileType (exe)"
STATUS=$(curl -s -o /dev/null -w "%{http_code}" --max-time 10 \
    -X POST "$API_BASE_URL/api/files/upload-url" \
    -H "Authorization: Bearer $CLIENT_TOKEN" \
    -H "Content-Type: application/json" \
    -d '{"fileName":"virus.exe","fileType":"exe","context":"DOCUMENT"}' 2>/dev/null)
assert_status "400" "$STATUS" "upload-url invalid fileType exe"

# ══════════════════════════════════════════════════════════════
# TEST 9 — POST /api/files/upload-url invalid context → 400
# ══════════════════════════════════════════════════════════════
print_section "Test 9 — upload-url invalid context (UNKNOWN)"
STATUS=$(curl -s -o /dev/null -w "%{http_code}" --max-time 10 \
    -X POST "$API_BASE_URL/api/files/upload-url" \
    -H "Authorization: Bearer $CLIENT_TOKEN" \
    -H "Content-Type: application/json" \
    -d '{"fileName":"file.pdf","fileType":"pdf","context":"UNKNOWN"}' 2>/dev/null)
assert_status "400" "$STATUS" "upload-url invalid context UNKNOWN"

# ══════════════════════════════════════════════════════════════
# TEST 10 — GET /api/files/view-url without token → 401
# ══════════════════════════════════════════════════════════════
print_section "Test 10 — GET /api/files/view-url sans token"
STATUS=$(curl -s -o /dev/null -w "%{http_code}" --max-time 10 \
    -X GET "$API_BASE_URL/api/files/view-url?objectKey=documents/test/file.pdf" 2>/dev/null)
assert_status "401" "$STATUS" "view-url without token"

# ══════════════════════════════════════════════════════════════
# TEST 11 — GET /api/files/view-url owner (CLIENT)
# ══════════════════════════════════════════════════════════════
print_section "Test 11 — view-url owner (CLIENT)"
if [ -n "$SAVED_OBJECT_KEY" ]; then
    RESPONSE=$(curl -s -w "\n%{http_code}" --max-time 10 \
        -X GET "$API_BASE_URL/api/files/view-url?objectKey=$SAVED_OBJECT_KEY" \
        -H "Authorization: Bearer $CLIENT_TOKEN" 2>/dev/null)
    STATUS=$(echo "$RESPONSE" | tail -1)
    BODY=$(echo "$RESPONSE" | sed '$d')

    assert_status "200" "$STATUS" "view-url owner CLIENT"

    if [ "$STATUS" = "200" ]; then
        VIEW_URL=$(json_get "$BODY" "viewUrl")
        VIEW_EXPIRES=$(json_get "$BODY" "expiresInSeconds")

        if [ -n "$VIEW_URL" ]; then
            pass "viewUrl is not empty"
        else
            fail "viewUrl is empty"
        fi

        if [ "$VIEW_EXPIRES" = "300" ]; then
            pass "expiresInSeconds = 300"
        else
            fail "expiresInSeconds expected 300, got $VIEW_EXPIRES"
        fi
    fi
else
    skip "view-url owner — no objectKey saved from Test 4"
fi

# ══════════════════════════════════════════════════════════════
# TEST 12 — GET /api/files/view-url ADMIN on CLIENT objectKey
# ══════════════════════════════════════════════════════════════
print_section "Test 12 — view-url ADMIN on CLIENT objectKey"
if [ -n "$SAVED_OBJECT_KEY" ]; then
    RESPONSE=$(curl -s -w "\n%{http_code}" --max-time 10 \
        -X GET "$API_BASE_URL/api/files/view-url?objectKey=$SAVED_OBJECT_KEY" \
        -H "Authorization: Bearer $ADMIN_TOKEN" 2>/dev/null)
    STATUS=$(echo "$RESPONSE" | tail -1)

    assert_status "200" "$STATUS" "view-url ADMIN can view CLIENT file"
else
    skip "view-url ADMIN — no objectKey saved from Test 4"
fi

# ══════════════════════════════════════════════════════════════
# TEST 13 — GET /api/files/view-url non-owner non-admin → 403
# ══════════════════════════════════════════════════════════════
print_section "Test 13 — view-url non-owner non-admin"
# Use admin's objectKey with client token to simulate non-owner access.
# First generate an objectKey belonging to admin.
ADMIN_RESPONSE=$(curl -s -w "\n%{http_code}" --max-time 10 \
    -X POST "$API_BASE_URL/api/files/upload-url" \
    -H "Authorization: Bearer $ADMIN_TOKEN" \
    -H "Content-Type: application/json" \
    -d '{"fileName":"admin-doc.pdf","fileType":"pdf","context":"DOCUMENT"}' 2>/dev/null)
ADMIN_BODY_STATUS=$(echo "$ADMIN_RESPONSE" | tail -1)
ADMIN_BODY=$(echo "$ADMIN_RESPONSE" | sed '$d')

if [ "$ADMIN_BODY_STATUS" = "200" ]; then
    ADMIN_OBJECT_KEY=$(json_get "$ADMIN_BODY" "objectKey")

    if [ -n "$ADMIN_OBJECT_KEY" ]; then
        STATUS=$(curl -s -o /dev/null -w "%{http_code}" --max-time 10 \
            -X GET "$API_BASE_URL/api/files/view-url?objectKey=$ADMIN_OBJECT_KEY" \
            -H "Authorization: Bearer $CLIENT_TOKEN" 2>/dev/null)
        assert_status "403" "$STATUS" "view-url non-owner CLIENT on ADMIN objectKey"
    else
        skip "view-url non-owner — could not extract admin objectKey"
    fi
else
    skip "view-url non-owner — could not generate admin objectKey (HTTP $ADMIN_BODY_STATUS)"
fi

# ══════════════════════════════════════════════════════════════
# TEST 14 — PUT file to presigned upload URL
# ══════════════════════════════════════════════════════════════
print_section "Test 14 — PUT file to presigned upload URL (optional)"
if [ -n "$SAVED_UPLOAD_URL" ]; then
    TMP_FILE=$(mktemp /tmp/wasel-minio-test-XXXXXX.txt 2>/dev/null || echo "/tmp/wasel-minio-test.txt")
    echo "wasel test file content" > "$TMP_FILE"

    UPLOAD_STATUS=$(curl -s -o /dev/null -w "%{http_code}" --max-time 10 \
        -X PUT "$SAVED_UPLOAD_URL" \
        -H "Content-Type: application/pdf" \
        --upload-file "$TMP_FILE" 2>/dev/null)

    rm -f "$TMP_FILE" 2>/dev/null

    if [ "$UPLOAD_STATUS" = "200" ] || [ "$UPLOAD_STATUS" = "204" ]; then
        pass "PUT to presigned URL (HTTP $UPLOAD_STATUS)"
    else
        fail "PUT to presigned URL failed (HTTP $UPLOAD_STATUS)"
        echo "  If testing locally, ensure MinIO is accessible on PublicEndpoint (e.g. localhost:9000)."
    fi
else
    skip "PUT upload — no upload URL saved from Test 4"
fi

# ══════════════════════════════════════════════════════════════
# SUMMARY
# ══════════════════════════════════════════════════════════════
print_summary
exit $TESTS_FAILED
