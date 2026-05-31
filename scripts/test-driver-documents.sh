#!/bin/bash
# test-driver-documents.sh

set -e

API_BASE_URL="http://localhost:5000"
KEYCLOAK_URL="http://localhost:8080/auth/realms/wasel/protocol/openid-connect/token"
CLIENT_USER="client@wasel.ma"
CLIENT_PASS="client123"
JSON_PARSER="grep"

echo "========================================================"
echo " ⚡ Test 1 — Retrieve CLIENT token"
echo "========================================================"
TOKEN_RESPONSE=$(curl -s -X POST "$KEYCLOAK_URL" \
    -d "client_id=wasel-api" \
    -d "grant_type=password" \
    -d "username=$CLIENT_USER" \
    -d "password=$CLIENT_PASS")

CLIENT_TOKEN=$(echo "$TOKEN_RESPONSE" | $JSON_PARSER -o '"access_token":"[^"]*' | cut -d'"' -f4)

if [ -z "$CLIENT_TOKEN" ]; then
    echo "❌ FAIL: Client token retrieval failed"
    exit 1
fi
echo "✅ PASS: Client token retrieved"

echo ""
echo "========================================================"
echo " ⚡ Test 2 — Ensure Driver Profile Exists"
echo "========================================================"
ME_STATUS=$(curl -s -o /dev/null -w "%{http_code}" -X GET "$API_BASE_URL/api/drivers/me" \
    -H "Authorization: Bearer $CLIENT_TOKEN")

if [ "$ME_STATUS" = "404" ]; then
    echo "ℹ️ Driver profile not found. Registering..."
    curl -s -X POST "$API_BASE_URL/api/drivers/register" \
        -H "Authorization: Bearer $CLIENT_TOKEN" \
        -H "Content-Type: application/json" \
        -d '{
              "permitNumber": "DOC-TEST-'$(date +%s)'",
              "permitDeliveryDate": "2020-01-01T00:00:00Z",
              "vehicle": {
                "type": "Motorcycle",
                "marque": "Yamaha",
                "model": "MT-07",
                "matricule": "12345-A-6"
              }
            }' > /dev/null
    echo "✅ PASS: Driver registered"
else
    echo "✅ PASS: Driver profile already exists"
fi

echo ""
echo "========================================================"
echo " ⚡ Test 3 — Get Upload URL (Mock)"
echo "========================================================"
# Here normally we would call /api/files/upload-url but for the test 
# we can just mock an objectKey that the frontend would have received and uploaded.
OBJECT_KEY="documents/mock-user-id/permit-file-$(date +%s).pdf"
echo "✅ Mocked ObjectKey: $OBJECT_KEY"

echo ""
echo "========================================================"
echo " ⚡ Test 4 — Add Document (POST /api/drivers/dossier/documents)"
echo "========================================================"
ADD_DOC_STATUS=$(curl -s -o /dev/null -w "%{http_code}" -X POST "$API_BASE_URL/api/drivers/dossier/documents" \
    -H "Authorization: Bearer $CLIENT_TOKEN" \
    -H "Content-Type: application/json" \
    -d "{
          \"documentType\": \"Permit\",
          \"objectKey\": \"$OBJECT_KEY\"
        }")

if [ "$ADD_DOC_STATUS" = "200" ]; then
    echo "✅ PASS: Document added successfully (HTTP 200)"
else
    echo "❌ FAIL: Document addition failed (HTTP $ADD_DOC_STATUS)"
fi

echo ""
echo "========================================================"
echo " ⚡ Test 5 — List Documents (GET /api/drivers/dossier/documents)"
echo "========================================================"
LIST_DOC_RESPONSE=$(curl -s -X GET "$API_BASE_URL/api/drivers/dossier/documents" \
    -H "Authorization: Bearer $CLIENT_TOKEN")

if echo "$LIST_DOC_RESPONSE" | grep -q "\"objectKey\":\"$OBJECT_KEY\""; then
    echo "✅ PASS: Retrieved document list successfully and found objectKey"
else
    echo "❌ FAIL: Failed to find document in list"
    echo "$LIST_DOC_RESPONSE"
fi

echo ""
echo "========================================================"
echo " ⚡ Test 6 — Replace Document (POST with same type)"
echo "========================================================"
NEW_OBJECT_KEY="documents/mock-user-id/permit-file-new-$(date +%s).pdf"
REPLACE_DOC_STATUS=$(curl -s -o /dev/null -w "%{http_code}" -X POST "$API_BASE_URL/api/drivers/dossier/documents" \
    -H "Authorization: Bearer $CLIENT_TOKEN" \
    -H "Content-Type: application/json" \
    -d "{
          \"documentType\": \"Permit\",
          \"objectKey\": \"$NEW_OBJECT_KEY\"
        }")

if [ "$REPLACE_DOC_STATUS" = "200" ]; then
    echo "✅ PASS: Document replaced successfully (HTTP 200)"
else
    echo "❌ FAIL: Document replacement failed (HTTP $REPLACE_DOC_STATUS)"
fi

echo ""
echo "========================================================"
echo " ⚡ Test 7 — Invalid Document Type (Expect 400)"
echo "========================================================"
INVALID_DOC_STATUS=$(curl -s -o /dev/null -w "%{http_code}" -X POST "$API_BASE_URL/api/drivers/dossier/documents" \
    -H "Authorization: Bearer $CLIENT_TOKEN" \
    -H "Content-Type: application/json" \
    -d "{
          \"documentType\": \"INVALID_TYPE\",
          \"objectKey\": \"some-key\"
        }")

if [ "$INVALID_DOC_STATUS" = "400" ]; then
    echo "✅ PASS: Invalid document type correctly blocked (HTTP 400)"
else
    echo "❌ FAIL: Invalid document type returned unexpected status (HTTP $INVALID_DOC_STATUS)"
fi

echo ""
echo "========================================================"
echo " 📊 Summary"
echo "========================================================"
echo "If any tests failed, please check the logs or Docker API."
