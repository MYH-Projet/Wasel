#!/bin/bash
set -euo pipefail

# Couleurs pour l'affichage
GREEN='\033[0;32m'
RED='\033[0;31m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

echo -e "${BLUE}==================================================${NC}"
echo -e "${BLUE}  E2E Runtime Test - Wasel Notification Service   ${NC}"
echo -e "${BLUE}==================================================${NC}"

# 1. Démarrer les services nécessaires
echo -e "\n${YELLOW}1. Démarrage des services Docker...${NC}"
if [ -f .env ]; then
    ENV_OPT=""
else
    ENV_OPT="--env-file .env.example"
    echo -e "${YELLOW}ℹ️ .env not found, using .env.example${NC}"
fi
docker compose $ENV_OPT up -d --build wasel-rabbitmq wasel-api wasel-notification-service

# 2. Vérifier RabbitMQ
echo -e "\n${YELLOW}2. Attente de RabbitMQ...${NC}"
max_retries=15
counter=0
until docker compose exec -T wasel-rabbitmq rabbitmq-diagnostics -q ping > /dev/null 2>&1; do
    sleep 2
    counter=$((counter + 1))
    if [ $counter -ge $max_retries ]; then
        echo -e "${RED}❌ Timeout en attendant RabbitMQ.${NC}"
        docker compose logs --tail=50 wasel-rabbitmq
        exit 1
    fi
done
echo -e "${GREEN}✓ RabbitMQ est prêt.${NC}"

# 3. Vérifier Wasel.Api
echo -e "\n${YELLOW}3. Attente de l'API (wasel-api)...${NC}"
API_BASE_URL=""
max_retries=30
counter=0

while [ $counter -lt $max_retries ]; do
    if curl -fsS http://localhost:8000/api/health > /dev/null 2>&1; then
        API_BASE_URL="http://localhost:8000"
        break
    elif curl -fsS http://localhost:5000/api/health > /dev/null 2>&1; then
        API_BASE_URL="http://localhost:5000"
        break
    elif curl -fsS http://localhost/api/health > /dev/null 2>&1; then
        API_BASE_URL="http://localhost"
        break
    fi
    sleep 2
    counter=$((counter + 1))
done

if [ -z "$API_BASE_URL" ]; then
    echo -e "${RED}❌ Timeout en attendant l'API.${NC}"
    docker compose logs --tail=50 wasel-api
    exit 1
fi
echo -e "${GREEN}✓ API est prête sur $API_BASE_URL.${NC}"

# 4. Vérifier NotificationService
echo -e "\n${YELLOW}4. Vérification de Wasel.NotificationService...${NC}"
sleep 5 # Laisser le temps au service de se connecter à RabbitMQ
NS_STATUS=$(docker compose ps wasel-notification-service --format json | grep '"State":"running"' || true)
if [ -z "$NS_STATUS" ]; then
    echo -e "${RED}❌ Le NotificationService n'est pas en cours d'exécution.${NC}"
    docker compose logs --tail=50 wasel-notification-service
    exit 1
fi
echo -e "${GREEN}✓ NotificationService est UP.${NC}"

# 5. Envoyer l'événement
echo -e "\n${YELLOW}5. Publication d'un test event avec un vrai utilisateur...${NC}"
TEST_USER_ID=$(docker compose exec -T wasel-postgres psql -U wasel_user -d wasel_db -t -A -c "SELECT \"Id\" FROM users LIMIT 1;" | xargs || true)
if [ -z "$TEST_USER_ID" ]; then
    echo -e "${RED}❌ FAIL: No user found in database. Run auth sync or seed users before E2E test.${NC}"
    exit 1
else
    echo -e "${GREEN}✓ Utilisateur trouvé en base : $TEST_USER_ID${NC}"
fi

RESPONSE=$(curl -sS -X POST "$API_BASE_URL/api/dev/events/test-notification" \
    -H "Content-Type: application/json" \
    -d "{
        \"recipientUserId\": \"$TEST_USER_ID\",
        \"title\": \"E2E RabbitMQ Test\",
        \"message\": \"Hello from Wasel.Api to NotificationService\"
    }" || true)

echo "Réponse API: $RESPONSE"

if [[ "$RESPONSE" != *"NotificationRequestedEvent published successfully"* ]]; then
    echo -e "${RED}❌ L'API n'a pas pu publier l'événement.${NC}"
    docker compose logs --tail=50 wasel-api
    exit 1
fi
echo -e "${GREEN}✓ Événement publié avec succès.${NC}"

# 6. Vérifier la consommation dans les logs du NotificationService
echo -e "\n${YELLOW}6. Vérification de la consommation...${NC}"
sleep 5

LOGS=$(docker compose logs --since=2m wasel-notification-service)
if echo "$LOGS" | grep -q "Processing NotificationEvent"; then
    echo -e "${GREEN}✓ Événement reçu et processé par le NotificationService !${NC}"
else
    echo -e "${RED}❌ L'événement ne semble pas avoir été processé.${NC}"
    echo "Logs du NotificationService :"
    echo "$LOGS"
    
    echo -e "\n${YELLOW}Diagnostic RabbitMQ:${NC}"
    docker compose exec -T wasel-rabbitmq rabbitmqctl list_queues name messages consumers
    exit 1
fi

# Vérifier absence erreur FK
if echo "$LOGS" | grep -q "violates foreign key constraint"; then
    echo -e "${RED}❌ L'événement a été consommé mais a échoué en base de données (FK constraint).${NC}"
    echo "$LOGS" | grep -B 2 -A 5 "violates foreign key constraint"
    exit 1
fi

# 7. Vérifier RabbitMQ routing
echo -e "\n${YELLOW}7. Vérification du routing RabbitMQ...${NC}"
QUEUES=$(docker compose exec -T wasel-rabbitmq rabbitmqctl list_queues name messages consumers)
echo "$QUEUES" | grep "notification.requested" || echo -e "${RED}Attention: file notification.requested introuvable${NC}"

# 8. Vérifier la BDD
echo -e "\n${YELLOW}8. Vérification de la persistance (PostgreSQL)...${NC}"
DB_CHECK=$(docker compose exec -T wasel-postgres psql -U wasel_user -d wasel_db -t -c "SELECT \"Id\", \"UserId\", \"Title\", \"Status\", \"CreatedAt\" FROM notifications WHERE \"Title\" = 'E2E RabbitMQ Test' ORDER BY \"CreatedAt\" DESC LIMIT 1;" || true)

if [ -n "$DB_CHECK" ] && echo "$DB_CHECK" | grep -q "E2E RabbitMQ Test"; then
    echo -e "${GREEN}✓ Notification sauvegardée en BDD avec succès :${NC}"
    echo "$DB_CHECK"
else
    echo -e "${RED}❌ FAIL: Notification non trouvée en BDD ou migration non appliquée.${NC}"
    echo "Résultat de la requête : $DB_CHECK"
    exit 1
fi

echo -e "\n${GREEN}==================================================${NC}"
echo -e "${GREEN} ✅ PASS: NotificationService E2E runtime test succeeded ${NC}"
echo -e "${GREEN}==================================================${NC}"
exit 0
