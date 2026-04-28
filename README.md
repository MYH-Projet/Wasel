# 🚚 Wasel — Plateforme de livraison à la demande

Wasel est une plateforme de livraison à la demande construite avec une architecture **monolithique modulaire**, prête à évoluer vers les microservices.

---

## 📋 Stack technique

| Composant | Technologie | Version |
|-----------|-------------|---------|
| Backend | ASP.NET Core | .NET 10 LTS |
| ORM | Entity Framework Core | 10.x |
| Database | PostgreSQL | 18.3 |
| Cache | Redis | 8.6 |
| Object Storage | MinIO | RELEASE.2025-02-28 |
| IAM / Auth | Keycloak | latest |
| API Docs | Scalar | 2.14 |
| Mobile | Flutter | — |
| Web | Astro | — |

---

## 🚀 Démarrage rapide

### Prérequis

- [Docker](https://www.docker.com/) (v20+)
- [Docker Compose](https://docs.docker.com/compose/) (v2+)
- [.NET 10 SDK](https://dotnet.microsoft.com/) (pour le développement local)

### 1. Configurer l'environnement

```bash
# Copier le fichier d'environnement
cp .env.example .env
```

### 2. Lancer l'infrastructure

```bash
docker compose up -d --build
```

### 3. Voir l'état et les logs
```bash
docker compose ps
docker compose logs -f wasel-api
```

### 4. Arrêter l'infrastructure

```bash
docker compose down
```

### 5. Arrêter et supprimer les données

```bash
docker compose down -v
```

---

## 🌐 URLs utiles

| Service | URL |
|---------|-----|
| **API** | http://localhost:5000 |
| **API Docs (Scalar)** | http://localhost:5000/scalar/v1 |
| **OpenAPI JSON** | http://localhost:5000/openapi/v1.json |
| **Health Check** | http://localhost:5000/api/health |
| **Keycloak (Auth)** | http://localhost:8080 |
| **Adminer** (DB UI) | http://localhost:8081 |
| **MinIO Console** | http://localhost:9001 |

---

## 🔑 Identifiants de développement

### PostgreSQL
| Champ | Valeur |
|-------|--------|
| Host | `localhost` |
| Port | `5432` |
| Database | `wasel_db` |
| Username | `wasel_user` |
| Password | `wasel_dev_password` |

### MinIO
| Champ | Valeur |
|-------|--------|
| Endpoint | `localhost:9000` |
| Console | `localhost:9001` |
| Access Key | `minioadmin` |
| Secret Key | `minioadmin123` |
| Bucket | `wasel-documents` |

### Keycloak
| Champ | Valeur |
|-------|--------|
| Admin Console | `http://localhost:8080` |
| Username | `admin` |
| Password | `admin` |

> ⚠️ **Notes Importantes :**
> - **Sécurité** : Ces identifiants sont uniquement pour le développement local. Ne jamais les utiliser en production. Ne pas commiter le fichier `.env`.
> - **Keycloak** : Keycloak est démarré dans Docker mais le module Auth complet sera développé dans une tâche suivante.
> - **MinIO** : Utilisé pour le développement local avec une image figée. Le bucket `wasel-documents` peut nécessiter une création manuelle via la console pour le moment.

---

## 📁 Structure du projet

```
wasel/
├── backend/
│   └── Wasel.Api/                  # API .NET 10 (monolithe modulaire)
│       ├── Modules/                # Modules métier
│       │   ├── Auth/               # Authentification
│       │   ├── Users/              # Gestion des utilisateurs
│       │   ├── Drivers/            # Gestion des chauffeurs
│       │   ├── Deliveries/         # Gestion des livraisons
│       │   ├── Payments/           # Paiements
│       │   ├── Documents/          # Documents et fichiers
│       │   ├── Tracking/           # Suivi en temps réel
│       │   └── Reviews/            # Avis et évaluations
│       ├── Shared/                 # Code partagé
│       │   ├── Common/             # Classes de base
│       │   ├── Database/           # DbContext EF Core
│       │   ├── Exceptions/         # Exceptions custom
│       │   ├── Responses/          # Format de réponse API
│       │   └── Security/           # Sécurité
│       └── Infrastructure/         # Intégrations externes
│           ├── Keycloak/           # IAM / Auth
│           ├── Redis/              # Cache
│           ├── MinIO/              # Object Storage
│           ├── Email/              # Emails
│           └── Maps/               # Géolocalisation
├── mobile/                         # Application Flutter
├── web/                            # Application Astro
├── infra/                          # Scripts d'infrastructure
│   ├── minio/
│   ├── keycloak/
│   └── scripts/
├── docker-compose.yml
├── .env.example
└── .gitignore
```

### Structure d'un module

Chaque module métier suit cette organisation :

```
Modules/<NomModule>/
├── Controllers/        # Endpoints API
├── DTOs/              # Data Transfer Objects
├── Entities/          # Entités EF Core
├── Enums/             # Énumérations
├── Services/          # Logique métier
├── Repositories/      # Accès aux données
└── Configurations/    # Configuration EF Core
```

---

## 🛠️ Développement local (sans Docker)

```bash
# Restaurer les packages
cd backend/Wasel.Api
dotnet restore

# Lancer le backend
dotnet run

# Ou en mode watch
dotnet watch run
```

> 💡 Assurez-vous que PostgreSQL, Redis et MinIO sont accessibles localement.

---

## 📝 Licence

Projet académique — Tous droits réservés.
