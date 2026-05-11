# 🧪 Tests et CI/CD — Wasel

Ce document explique la stratégie de tests automatisés et la CI/CD mise en place pour le projet Wasel.

---

## 📋 Types de tests

### 1. Tests unitaires backend (xUnit)

**Projet** : `backend/Wasel.Api.Tests/`

Tests rapides qui vérifient la logique métier **sans dépendance externe** (pas de PostgreSQL, pas de Redis, pas de Keycloak, pas de Docker).

| Fichier de test | Ce qu'il vérifie |
|---|---|
| `UserServiceTests.cs` | Création/synchronisation utilisateurs Keycloak, changement de statut, mise à jour du profil |
| `AuthServiceTests.cs` | Validation des tokens JWT, auto-sync utilisateur local, protection des endpoints |
| `KeycloakClaimsTransformerTests.cs` | Conversion des rôles Keycloak (realm_access) en ClaimTypes.Role |
| `HealthEndpointTests.cs` | Test d'intégration léger : l'endpoint `/api/health` retourne HTTP 200 |

**Pourquoi les tests unitaires ne dépendent pas de Docker ?**

Les tests utilisent des **fakes in-memory** : des classes C# qui simulent le comportement de la base de données avec une simple `List<User>` en mémoire. Cela permet de :
- Exécuter les tests en **millisecondes** (pas de conteneur à démarrer)
- Les lancer **n'importe où** (PC local, CI GitHub Actions, etc.)
- Tester la **logique métier pure** sans bruit réseau ou configuration

### 2. Tests frontend web (Vitest)

**Projet** : `web/`

Tests de la fonction utilitaire `cn()` (`src/lib/utils.ts`) qui gère le merge des classes Tailwind CSS.

### 3. Tests mobile Flutter

**Projet** : `mobile/`

Test widget vérifiant que l'application affiche bien "Hello World!".

### 4. Tests d'intégration manuels (Docker + Keycloak)

**Script** : `scripts/test-auth.sh`

Tests complets nécessitant Docker Compose avec Keycloak actif. Ce script teste le flux d'authentification de bout en bout (obtention de token, auto-sync, permissions admin/client, etc.).

> ⚠️ Ce script est **optionnel** et n'est pas exécuté par la CI. Il reste utile pour valider l'intégration complète en développement local.

---

## 🚀 Commandes locales

### Backend
```bash
# Restaurer les dépendances
dotnet restore Wasel.sln

# Compiler
dotnet build Wasel.sln --configuration Release

# Lancer les tests
dotnet test backend/Wasel.Api.Tests/Wasel.Api.Tests.csproj --configuration Release --verbosity normal
```

### Frontend web
```bash
cd web
npm ci
npm run build
npm test
```

### Mobile Flutter
```bash
cd mobile
flutter pub get
flutter analyze
flutter test
```

### Validation Docker Compose
```bash
docker compose config --quiet
```

### Tests d'intégration manuels (nécessite Docker)
```bash
# Démarrer l'infrastructure
docker compose up -d --build

# Lancer les tests d'intégration
bash scripts/test-auth.sh

# Ou via Nginx
API_BASE_URL=http://localhost KEYCLOAK_URL=http://localhost/auth bash scripts/test-auth.sh
```

---

## ⚙️ CI/CD GitHub Actions

### Workflow principal : `ci.yml`

Se déclenche sur :
- `pull_request` vers `dev` et `main`
- `push` vers `dev`

| Job | Description |
|---|---|
| `backend` | Restore → Build → **dotnet test** (xUnit) |
| `web` | npm ci → npm run build → **npm test** (Vitest) |
| `mobile` | flutter pub get → flutter analyze → **flutter test** |
| `docker-validation` | docker compose config --quiet |

**Important** : La CI **échoue** si les tests échouent. Il n'y a plus de logique "skip if no tests".

### Workflow Docker : `backend-docker.yml`

Se déclenche sur `push` vers `main`. Construit et pousse l'image Docker du backend vers Docker Hub.

> 💡 **Recommandation** : Configurer les [branch protection rules](https://docs.github.com/en/repositories/configuring-branches-and-merges-in-your-repository/managing-protected-branches/about-protected-branches) sur GitHub pour exiger le succès du workflow CI avant tout merge sur `main`. Cela empêche de pousser une image Docker si les tests échouent.

### Workflow backend legacy : `backend-ci.yml`

Conservé pour compatibilité. Exécute maintenant les tests backend explicitement (plus de skip silencieux).

---

## 🔮 Améliorations futures

Les éléments suivants ne sont **pas encore implémentés** mais sont prévus pour les prochaines phases :

| Amélioration | Description |
|---|---|
| **Tests E2E avec Playwright** | Tests navigateur automatisés pour le frontend web |
| **Couverture de code** | Générer des rapports de couverture (Coverlet pour .NET, c8 pour Vitest) |
| **Tests d'intégration CI avec Keycloak** | Démarrer Keycloak en conteneur dans la CI pour exécuter `test-auth.sh` automatiquement |
| **Tests API complets** | Tester tous les endpoints avec `WebApplicationFactory` et authentification simulée |
| **Déploiement staging** | Déployer automatiquement sur un environnement staging après succès CI |
| **Tests mobile avancés** | Tests d'intégration Flutter avec navigation et appels API |
