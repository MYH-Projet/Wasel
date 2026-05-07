# 🚚 Wasel — Plateforme de Livraison à la Demande

> Backend .NET 10 · Keycloak · PostgreSQL · Redis · MinIO · Nginx

[![Build Status](https://github.com/MYH-Projet/Wasel/actions/workflows/backend-ci.yml/badge.svg)](https://github.com/MYH-Projet/Wasel/actions)

---

## 📋 Vue d'ensemble

**Wasel** est une plateforme de livraison à la demande connectant :
- 👤 **Clients** — passent des commandes de livraison
- 🛵 **Livreurs (Drivers)** — acceptent et effectuent les livraisons
- 🛡️ **Administrateurs** — valident les comptes et supervisent la plateforme

---

## 🏗️ Architecture

```
Flutter (Mobile) / Astro (Web Admin)
              ↓
      ┌───────────────┐
      │  Nginx  :80   │  ← Point d'entrée unique
      │  /api/*       │→ Backend .NET (wasel-api:8080)
      │  /auth/*      │→ Keycloak     (wasel-keycloak:8080)
      └───────────────┘
              ↓
      ┌───────────────────────────────────┐
      │         Backend .NET 10           │
      │  Monolithe Modulaire              │
      │  Modules: Auth, Users, Drivers... │
      └───────────────────────────────────┘
         ↓           ↓           ↓
    PostgreSQL     Redis       MinIO
    (données)    (cache)    (fichiers)
```

---

## 🚀 Démarrage rapide

### Prérequis
- [Docker Desktop](https://www.docker.com/products/docker-desktop/)
- [Git](https://git-scm.com/)

### Lancer le projet

```bash
# 1. Cloner le repository
git clone https://github.com/MYH-Projet/Wasel.git
cd Wasel

# 2. Configurer l'environnement
cp .env.example .env

# 3. Lancer tous les services
docker compose up -d --build

# 4. Vérifier que tout fonctionne
curl http://localhost/api/health        # via Nginx
curl http://localhost:5000/api/health   # accès direct
```

### URLs de développement

| Service | Accès Direct | Via Nginx |
|---|---|---|
| 🔌 **API** | `http://localhost:5000/api` | `http://localhost/api` |
| 📖 **Docs API (Scalar)** | `http://localhost:5000/scalar/v1` | `http://localhost/scalar/v1` |
| 🔑 **Keycloak** | `http://localhost:8080/auth` | `http://localhost/auth` |
| 💾 **Adminer (BDD)** | `http://localhost:8081` | — |
| 🗂️ **MinIO Console** | `http://localhost:9001` | — |

---

## 📡 Endpoints API

### Public
| `GET /api/health` | Santé de l'API |

### 🔐 Authentification (token requis)
| Méthode | Endpoint | Description |
|---|---|---|
| `GET` | `/api/auth/me` | Profil connecté (auto-sync) |
| `PATCH` | `/api/auth/me/profile` | Modifier le profil local |
| `POST` | `/api/auth/sync` | Sync manuelle (optionnel) |

### 👑 Administration (rôle ADMIN)
| Méthode | Endpoint | Description |
|---|---|---|
| `GET` | `/api/admin/users` | Liste des utilisateurs |
| `GET` | `/api/admin/users/{id}` | Détail utilisateur |
| `PATCH` | `/api/admin/users/{id}/status` | Changer le statut |

---

## 🧪 Tests

```bash
# Tests d'intégration auth (accès direct)
bash scripts/test-auth.sh

# Tests via Nginx
API_BASE_URL=http://localhost KEYCLOAK_URL=http://localhost/auth bash scripts/test-auth.sh
```

---

## 📁 Structure du projet

```
Wasel/
├── backend/Wasel.Api/       ← API .NET 10
│   ├── Modules/             ← Auth, Users, Drivers...
│   ├── Shared/              ← Code commun
│   └── Infrastructure/      ← Keycloak, MinIO, Redis
├── infra/
│   ├── nginx/nginx.conf     ← Reverse proxy
│   └── keycloak/            ← realm-export.json
├── scripts/
│   └── test-auth.sh         ← Suite de tests
├── docs/                    ← Documentation technique
└── docker-compose.yml
```

---

## 📚 Documentation

| Document | Description |
|---|---|
| [backend-guide.md](docs/backend-guide.md) | Architecture, règles, endpoints backend |
| [frontend-auth-guide.md](docs/frontend-auth-guide.md) | Intégration auth Flutter / Astro |

---

## 👥 Comptes de test (dev uniquement)

| Utilisateur | Mot de passe | Rôle |
|---|---|---|
| `admin@wasel.ma` | `admin123` | ADMIN |
| `client@wasel.ma` | `client123` | CLIENT |
