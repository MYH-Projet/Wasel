$jsonPayload = @{
    eventId = "11111111-1111-1111-1111-111111111111"
    recipientUserId = "22222222-2222-2222-2222-222222222222"
    type = "DELIVERY_ASSIGNED"
    title = "Nouvelle livraison assignée"
    message = "Une nouvelle livraison vous a été assignée."
    relatedEntityType = "DELIVERY"
    relatedEntityId = "33333333-3333-3333-3333-333333333333"
    channels = @("IN_APP", "PUSH")
    createdAt = "2026-06-04T10:00:00Z"
} | ConvertTo-Json

$rabbitPayload = @{
    properties = @{}
    routing_key = "notification.requested"
    payload = $jsonPayload
    payload_encoding = "string"
} | ConvertTo-Json -Depth 10

$headers = @{
    Authorization = "Basic " + [Convert]::ToBase64String([Text.Encoding]::ASCII.GetBytes("wasel:wasel"))
    "Content-Type" = "application/json"
}

Invoke-RestMethod -Uri "http://localhost:15672/api/exchanges/%2F/wasel.events/publish" -Method Post -Headers $headers -Body $rabbitPayload
