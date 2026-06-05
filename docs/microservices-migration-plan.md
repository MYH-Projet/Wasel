# Migration progressive vers microservices — Notification Service

## 1. Contexte
Wassel a été initialement construit comme un monolithe modulaire. L'objectif actuel est d'extraire progressivement le module de Notification (`Wasel.Api.Modules.Notifications`) vers un microservice indépendant (`Wasel.NotificationService`).

## 2. Architecture Cible Immédiate
Le flux d'information pour les notifications sera le suivant :
**`Wasel.Api`** → **`RabbitMQ`** → **`Wasel.NotificationService`**

## 3. Rôle de RabbitMQ
RabbitMQ sert de bus d'événements interne pour la communication asynchrone entre le monolithe et les microservices.
- **Exchange** : `wasel.events` (Type: `Direct`)
- **Routing Key** : `notification.requested`
- **Queue** : `notification.requested`

## 4. Rôle du NotificationService
Le `Wasel.NotificationService` est un Worker Service .NET conçu pour :
- Consommer les événements `NotificationRequestedEvent` depuis RabbitMQ.
- Persister l'historique de la notification dans la base de données (table `notifications`).
- Envoyer ou simuler l'envoi des notifications Push via Firebase.
- Envoyer des notifications par Email (actuellement préparé mais configuré en `Noop`).

*Note : NotificationService inclut une stratégie de retry au démarrage pour attendre RabbitMQ et éviter BrokerUnreachableException lors des redéploiements.*

## 5. Environnement Local
Pour développer et tester localement :
- Les services Docker nécessaires sont gérés via `docker-compose.yml`.
- RabbitMQ est exposé sur les ports `5672` (AMQP) et `15672` (Management UI - *login: wasel / pass: wasel*).
- Les variables de configuration sont définies dans le `.env` local (copié de `.env.example`).
- Commande de lancement : `docker compose up -d wasel-rabbitmq wasel-api wasel-notification-service`

## 6. Environnement Staging
L'environnement de staging sur le VPS est configuré pour refléter la production :
- RabbitMQ est exécuté dans un conteneur mais **n'est pas exposé publiquement** pour des raisons de sécurité.
- Le `Wasel.NotificationService` est tiré depuis l'image Docker Hub construite par GitHub Actions.
- La configuration se fait via le fichier `.env.staging` (dérivé de `.env.staging.example`).

## 7. Tests
Le flux complet peut être testé :
- En exécutant `dotnet build Wasel.sln` pour la compilation complète.
- Les tests du backend : `dotnet test backend/Wasel.Api.Tests/...`
- Les tests du microservice : `dotnet test services/Wasel.NotificationService.Tests/...`
- Un endpoint Dev est disponible sur le monolithe pour générer des événements de test : `POST /api/dev/events/test-notification`

## 8. Limites Actuelles
- **Double Notification** : Pendant la période de migration, les notifications in-app sont générées à la fois par le service monolithique et le microservice (qui écoute les événements).
- **Dead Letter Queue (DLQ)** : Il n'y a pas encore de mécanisme DLQ configuré pour les messages RabbitMQ qui échouent après plusieurs tentatives (ils sont actuellement rejetés avec requeue=true).
- **Firebase** : Par défaut, Firebase est désactivé (`FIREBASE_ENABLED=false`) pour éviter les erreurs si aucun compte de service n'est configuré.
- Les endpoints REST de consultation `/api/notifications` sont toujours servis par le monolithe. Le module Notifications monolithique n'est pas encore supprimé.

## 9. Prochaine Étape
- Valider le bon fonctionnement global en environnement de Staging.
- Implémenter une **Dead Letter Queue (DLQ)** dans RabbitMQ.
- Retirer la logique de création de notifications du monolithe (pour éviter la double notification).
- Déplacer ou proxifier les endpoints REST `/api/notifications` vers le microservice si nécessaire.
- Évaluer l'extraction du prochain module (ex: Files Service / Documents).
