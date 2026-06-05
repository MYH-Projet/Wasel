docker compose stop wasel-notification-service
sleep 5
docker compose exec -T wasel-rabbitmq rabbitmqctl purge_queue notification.requested
docker compose start wasel-notification-service
