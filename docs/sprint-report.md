# Rapport de Sprint — Wasel Backend

> Période : Avril – Mai 2026
> Équipe : Backend

---

## ✅ Ce qui a été réalisé

### 1. Infrastructure de base (`feature/setup-backend-architecture`)

- Projet .NET 10 structuré en **monolithe modulaire**
- 8 modules métier créés (`Auth`, `Users`, `Drivers`, `Deliveries`, `Payments`, `Documents`, `Tracking`, `Reviews`)
- Docker Compose complet avec 6 services
- Endpoint de santé `/api/health`
- Documentation API via Scalar
- Migrations EF Core automatiques au démarrage (dev)
- CI/CD GitHub Actions (build + docker push)

---

### 2. Authentification Keycloak (`feature/keycloak-setup`)

- Intégration JWT Bearer avec Keycloak
- Validation des rôles : `ADMIN`, `CLIENT`, `DRIVER`
- `KeycloakClaimsTransformer` — conversion `realm_access.roles` → `ClaimTypes.Role`
- `CurrentUserService` — extraction des claims du JWT courant
- Endpoints `/api/auth/me`, `/api/auth/sync`, `/api/auth/claims`
- Endpoints admin `/api/admin/users`, `/api/admin/users/{id}/status`
- Automatisation Keycloak via `realm-import.json`
- Script de test `scripts/test-auth.sh`

---

### 3. Auto-Sync utilisateur (`feature/auth-auto-sync-current-user`)

**Problème résolu :** Le frontend était obligé d'appeler `POST /api/auth/sync` avant chaque appel à `/api/auth/me`, sinon `404`.

**Solution :** Méthode `EnsureCurrentUserExistsAsync()` dans `AuthService`.

**Comportement :**
- `GET /api/auth/me` → crée automatiquement le profil local si absent
- `PATCH /api/auth/me/profile` → crée automatiquement le profil avant la mise à jour
- `POST /api/auth/sync` → conservé pour la compatibilité, utilise la même logique

**Impact frontend :** Le frontend n'a plus besoin d'appeler `/api/auth/sync`. Le flux est simplifié : Login → `GET /api/auth/me` → profil local garanti.

---

### 4. Nginx Reverse Proxy (`feature/nginx-reverse-proxy`)

**Objectif :** Point d'entrée unique pour les clients frontend.

**Architecture ajoutée :**
```
Nginx :80
├── /api/*  → wasel-api:8080
└── /auth/* → wasel-keycloak:8080
```

**Fichiers créés/modifiés :**
- `infra/nginx/nginx.conf` — configuration Nginx
- `docker-compose.yml` — service `wasel-nginx`, `KC_HTTP_RELATIVE_PATH=/auth`
- `KeycloakOptions.cs` — ajout `NginxAuthority`
- `Program.cs` — 3 issuers valides (direct, interne, Nginx)
- `appsettings.json` / `appsettings.Development.json` — URLs avec `/auth`
- `scripts/test-auth.sh` — `KEYCLOAK_URL` mis à jour

**Résultats des tests :**
| Mode | Résultat |
|---|---|
| Accès direct (`:5000` / `:8080`) | 17/17 PASS |
| Via Nginx (`:80`) | 17/17 PASS |

---

## 📊 État actuel de l'infrastructure

| Service | Status | Rôle |
|---|---|---|
| `wasel-nginx` | ✅ Opérationnel | Reverse proxy |
| `wasel-api` | ✅ Opérationnel | API .NET 10 |
| `wasel-keycloak` | ✅ Opérationnel | IAM / Auth |
| `wasel-postgres` | ✅ Opérationnel | Base de données |
| `wasel-redis` | ✅ Opérationnel | Cache |
| `wasel-minio` | ✅ Opérationnel | Stockage fichiers |
| `wasel-adminer` | ✅ Opérationnel | UI base de données |

---

## 🚧 Ce qui reste à faire (prochains sprints)

| Priorité | Module | Description |
|---|---|---|
| 🔴 Haute | `Drivers` | Inscription livreur, validation admin, statuts |
| 🔴 Haute | `Deliveries` | Création commande, affectation livreur, statuts |
| 🟡 Moyenne | `Documents` | Upload fichiers (MinIO) — permis, photos profil |
| 🟡 Moyenne | `Tracking` | Géolocalisation temps réel (Redis + WebSocket) |
| 🟢 Basse | `Payments` | Transactions, historique |
| 🟢 Basse | `Reviews` | Notes et avis |
| 🟢 Basse | Auth | Refresh token, logout Keycloak, reset password |
| 🟢 Basse | CI/CD | Déploiement automatique sur VPS |

---

## 🔒 Contraintes respectées

- ❌ Pas de `/api/auth/login` ni `/api/auth/register` — Keycloak gère l'identité
- ❌ Pas de secrets dans le code (`.env` exclu du Git)
- ❌ Pas de `catch (Exception)` générique masquant les erreurs DB
- ✅ `realm-import.json` non modifié pour l'auth directe
- ✅ Accès directs `:5000` et `:8080` préservés

---

## 📁 Branches Git

| Branche | Description | Status |
|---|---|---|
| `main` | Production stable | — |
| `dev` | Intégration | ✅ À jour |
| `feature/setup-backend-architecture` | Infrastructure de base | ✅ Mergé |
| `feature/keycloak-setup` | Authentification | ✅ Mergé |
| `feature/auth-auto-sync-current-user` | Auto-sync | ✅ Mergé |
| `feature/nginx-reverse-proxy` | Reverse proxy | 🔄 En cours (PR à créer) |
