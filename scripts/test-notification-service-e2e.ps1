$ErrorActionPreference = "Stop"

Write-Host "==================================================" -ForegroundColor Blue
Write-Host "  E2E Runtime Test - Wasel Notification Service   " -ForegroundColor Blue
Write-Host "==================================================" -ForegroundColor Blue

# 1. Démarrer les services
Write-Host "`n1. Démarrage des services Docker..." -ForegroundColor Yellow
docker compose up -d --build wasel-rabbitmq wasel-api wasel-notification-service

# 2. Vérifier RabbitMQ
Write-Host "`n2. Attente de RabbitMQ..." -ForegroundColor Yellow
$maxRetries = 15
$counter = 0
$isReady = $false

while ($counter -lt $maxRetries) {
    $result = docker compose exec -T wasel-rabbitmq rabbitmq-diagnostics -q ping 2>&1
    if ($LASTEXITCODE -eq 0) {
        $isReady = $true
        break
    }
    Start-Sleep -Seconds 2
    $counter++
}

if (-not $isReady) {
    Write-Host "❌ Timeout en attendant RabbitMQ." -ForegroundColor Red
    docker compose logs --tail=50 wasel-rabbitmq
    exit 1
}
Write-Host "✓ RabbitMQ est prêt." -ForegroundColor Green

# 3. Vérifier Wasel.Api
Write-Host "`n3. Attente de l'API (wasel-api)..." -ForegroundColor Yellow
$apiBaseUrl = ""
$maxRetries = 30
$counter = 0

while ($counter -lt $maxRetries) {
    try {
        $response = Invoke-WebRequest -Uri "http://localhost:8000/api/health" -Method Get -ErrorAction SilentlyContinue
        if ($response.StatusCode -eq 200) { $apiBaseUrl = "http://localhost:8000"; break }
    } catch {}
    try {
        $response = Invoke-WebRequest -Uri "http://localhost:5000/api/health" -Method Get -ErrorAction SilentlyContinue
        if ($response.StatusCode -eq 200) { $apiBaseUrl = "http://localhost:5000"; break }
    } catch {}
    
    Start-Sleep -Seconds 2
    $counter++
}

if ([string]::IsNullOrEmpty($apiBaseUrl)) {
    Write-Host "❌ Timeout en attendant l'API." -ForegroundColor Red
    docker compose logs --tail=50 wasel-api
    exit 1
}
Write-Host "✓ API est prête sur $apiBaseUrl." -ForegroundColor Green

# 4. Vérifier NotificationService
Write-Host "`n4. Vérification de Wasel.NotificationService..." -ForegroundColor Yellow
Start-Sleep -Seconds 5
$nsStatus = docker compose ps wasel-notification-service --format json | Select-String '"State":"running"'
if (-not $nsStatus) {
    Write-Host "❌ Le NotificationService n'est pas en cours d'exécution." -ForegroundColor Red
    docker compose logs --tail=50 wasel-notification-service
    exit 1
}
Write-Host "✓ NotificationService est UP." -ForegroundColor Green

# 5. Envoyer l'événement
Write-Host "`n5. Publication d'un événement de test..." -ForegroundColor Yellow
$body = @{
    recipientUserId = "00000000-0000-0000-0000-000000000001"
    title = "E2E RabbitMQ Test"
    message = "Hello from Wasel.Api to NotificationService"
} | ConvertTo-Json

try {
    $response = Invoke-RestMethod -Uri "$apiBaseUrl/api/dev/events/test-notification" -Method Post -Body $body -ContentType "application/json"
    Write-Host "Réponse API: $($response | ConvertTo-Json -Compress)"
} catch {
    Write-Host "❌ L'API n'a pas pu publier l'événement." -ForegroundColor Red
    Write-Host $_.Exception.Message
    exit 1
}
Write-Host "✓ Événement publié avec succès." -ForegroundColor Green

# 6. Vérification consommation
Write-Host "`n6. Vérification de la consommation..." -ForegroundColor Yellow
Start-Sleep -Seconds 5
$logs = docker compose logs --since=2m wasel-notification-service
if ($logs -match "Processing NotificationEvent") {
    Write-Host "✓ Événement reçu et processé par le NotificationService !" -ForegroundColor Green
} else {
    Write-Host "❌ L'événement ne semble pas avoir été processé." -ForegroundColor Red
    Write-Host "Logs du NotificationService :"
    Write-Host $logs
    
    Write-Host "`nDiagnostic RabbitMQ:" -ForegroundColor Yellow
    docker compose exec -T wasel-rabbitmq rabbitmqctl list_queues name messages consumers
    exit 1
}

# 7. Routing
Write-Host "`n7. Vérification du routing RabbitMQ..." -ForegroundColor Yellow
$queues = docker compose exec -T wasel-rabbitmq rabbitmqctl list_queues name messages consumers
Write-Host $queues
if ($queues -notmatch "notification.requested") {
    Write-Host "Attention: file notification.requested introuvable" -ForegroundColor Red
}

Write-Host "`n==================================================" -ForegroundColor Green
Write-Host " ✅ PASS: NotificationService E2E runtime test succeeded " -ForegroundColor Green
Write-Host "==================================================" -ForegroundColor Green
