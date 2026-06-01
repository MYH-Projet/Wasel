#!/bin/bash
# test-driver-onboarding.sh
# Tests Driver Onboarding Endpoints

echo "========================================================"
echo " ⚡ Test 1 — Retrieve CLIENT token"
echo "========================================================"

TOKEN=$(curl -s -X POST "http://localhost:8080/auth/realms/wasel/protocol/openid-connect/token" \
  -H "Content-Type: application/x-www-form-urlencoded" \
  -d "client_id=wasel-api" \
  -d "username=client@wasel.ma" \
  -d "password=client123" \
  -d "grant_type=password" | grep -o '"access_token":"[^"]*"' | cut -d'"' -f4)

if [ -z "$TOKEN" ]; then
  echo "❌ FAIL: Client token retrieval failed"
  exit 1
fi
echo "✅ PASS: Client token retrieved"

echo ""
echo "========================================================"
echo " ⚡ Test 1.5 — Trigger auto-sync and Set User Active"
echo "========================================================"
curl -s -o /dev/null -X GET "http://localhost:5000/api/auth/me" -H "Authorization: Bearer $TOKEN"

if command -v docker &> /dev/null; then
    docker exec wasel-postgres psql -U wasel_user -d wasel_db -t -c "UPDATE users SET \"Status\" = 1 WHERE \"Email\" = 'client@wasel.ma';" > /dev/null
    echo "✅ PASS: User status set to Active via DB"
else
    echo "⚠️ WARNING: Docker not found. Cannot force Active status via DB. Next tests might fail if ActiveUserOnly is enforced."
fi

echo ""
echo "========================================================"
echo " ⚡ Test 2 — Register Driver (POST /api/drivers/register)"
echo "========================================================"

REGISTER_HTTP_CODE=$(curl -s -o /dev/null -w "%{http_code}" -X POST "http://localhost:5000/api/drivers/register" \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
        "permisNumber": "B123456",
        "vehicle": {
            "type": "MOTORCYCLE",
            "matricule": "12345-A-6",
            "model": "Click 125",
            "marque": "Honda"
        }
    }')

if [ "$REGISTER_HTTP_CODE" -eq 200 ] || [ "$REGISTER_HTTP_CODE" -eq 201 ]; then
  echo "✅ PASS: Driver registered successfully (HTTP $REGISTER_HTTP_CODE)"
elif [ "$REGISTER_HTTP_CODE" -eq 409 ]; then
  echo "⚠️ WARNING: Driver already exists or PermitNumber in use (HTTP 409)"
else
  echo "❌ FAIL: Driver registration failed (HTTP $REGISTER_HTTP_CODE)"
fi

echo ""
echo "========================================================"
echo " ⚡ Test 3 — Get My Driver Profile (GET /api/drivers/me)"
echo "========================================================"

GET_ME_HTTP_CODE=$(curl -s -o /dev/null -w "%{http_code}" -X GET "http://localhost:5000/api/drivers/me" \
  -H "Authorization: Bearer $TOKEN")

if [ "$GET_ME_HTTP_CODE" -eq 200 ]; then
  echo "✅ PASS: Retrieved driver profile (HTTP 200)"
else
  echo "❌ FAIL: Failed to retrieve driver profile (HTTP $GET_ME_HTTP_CODE)"
fi

echo ""
echo "========================================================"
echo " ⚡ Test 4 — Register Driver Again (Expect 409)"
echo "========================================================"

REGISTER_AGAIN_HTTP_CODE=$(curl -s -o /dev/null -w "%{http_code}" -X POST "http://localhost:5000/api/drivers/register" \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
        "permisNumber": "B123456",
        "vehicle": {
            "type": "MOTORCYCLE",
            "matricule": "12345-A-6",
            "model": "Click 125",
            "marque": "Honda"
        }
    }')

if [ "$REGISTER_AGAIN_HTTP_CODE" -eq 409 ]; then
  echo "✅ PASS: Driver registration correctly blocked (HTTP 409)"
else
  echo "❌ FAIL: Driver registration returned unexpected status (HTTP $REGISTER_AGAIN_HTTP_CODE)"
fi

echo ""
echo "========================================================"
echo " ⚡ Test 5 — Submit Dossier (POST /api/drivers/dossier/submit)"
echo "========================================================"

SUBMIT_HTTP_CODE=$(curl -s -o /dev/null -w "%{http_code}" -X POST "http://localhost:5000/api/drivers/dossier/submit" \
  -H "Authorization: Bearer $TOKEN")

if [ "$SUBMIT_HTTP_CODE" -eq 200 ]; then
  echo "✅ PASS: Dossier submitted successfully (HTTP 200)"
elif [ "$SUBMIT_HTTP_CODE" -eq 400 ]; then
  echo "⚠️ WARNING: Dossier submission failed (HTTP 400) - Likely already submitted"
else
  echo "❌ FAIL: Dossier submission failed (HTTP $SUBMIT_HTTP_CODE)"
fi

echo ""
echo "========================================================"
echo " ⚡ Test 6 — Submit Dossier Again (Expect 400)"
echo "========================================================"

SUBMIT_AGAIN_HTTP_CODE=$(curl -s -o /dev/null -w "%{http_code}" -X POST "http://localhost:5000/api/drivers/dossier/submit" \
  -H "Authorization: Bearer $TOKEN")

if [ "$SUBMIT_AGAIN_HTTP_CODE" -eq 400 ]; then
  echo "✅ PASS: Dossier submission correctly blocked (HTTP 400)"
else
  echo "❌ FAIL: Dossier submission returned unexpected status (HTTP $SUBMIT_AGAIN_HTTP_CODE)"
fi

echo ""
echo "========================================================"
echo " ⚡ Test 7 — No Token (Expect 401)"
echo "========================================================"

NO_TOKEN_HTTP_CODE=$(curl -s -o /dev/null -w "%{http_code}" -X GET "http://localhost:5000/api/drivers/me")

if [ "$NO_TOKEN_HTTP_CODE" -eq 401 ]; then
  echo "✅ PASS: Unauthorized access blocked (HTTP 401)"
else
  echo "❌ FAIL: Unauthorized access returned unexpected status (HTTP $NO_TOKEN_HTTP_CODE)"
fi

echo ""
echo "========================================================"
echo " 📊 Summary"
echo "========================================================"
echo "If any tests failed, please check the Docker logs."
