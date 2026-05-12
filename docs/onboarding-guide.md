# Guide d'Onboarding — Nouveau Développeur Wasel

Bienvenue sur le projet Wasel ! Ce guide te permet d'être opérationnel en **moins de 15 minutes**.

---

## Étape 1 — Prérequis à installer

| Outil | Version minimale | Lien |
|---|---|---|
| Docker Desktop | ≥ 4.x | [docker.com](https://www.docker.com/products/docker-desktop/) |
| .NET SDK | 10.0 | [dot.net](https://dotnet.microsoft.com/download) |
| Git | Récent | [git-scm.com](https://git-scm.com/) |
| VS Code ou Rider | — | — |

---

## Étape 2 — Cloner et configurer

```bash
git clone https://github.com/MYH-Projet/Wasel.git
cd Wasel

# Copier la config d'environnement (ne pas commiter .env !)
cp .env.example .env
```

**Le fichier `.env` contient toutes les variables nécessaires.** Les valeurs par défaut fonctionnent directement pour le développement local.

---

## Étape 3 — Lancer l'infrastructure

```bash
docker compose up -d --build
```

Cette commande lance **7 services** :

| Service | Rôle | Port |
|---|---|---|
| `wasel-nginx` | Reverse proxy (point d'entrée) | `:80` |
| `wasel-api` | API .NET 10 | `:5000` |
| `wasel-keycloak` | Authentification (IAM) | `:8080` |
| `wasel-postgres` | Base de données | `:5432` |
| `wasel-redis` | Cache | `:6379` |
| `wasel-minio` | Stockage de fichiers | `:9000/9001` |
| `wasel-adminer` | Interface de BDD | `:8081` |

---

## Étape 4 — Vérifier que tout fonctionne

```bash
# Vérifier les conteneurs
docker compose ps

# Tester l'API
curl http://localhost/api/health
# → {"status":"Healthy","service":"Wasel.Api",...}
```

---

## Étape 5 — Explorer l'API

Ouvre **http://localhost:5000/scalar/v1** dans ton navigateur.

C'est la documentation interactive Scalar — tu peux tester tous les endpoints directement depuis le navigateur.

---

## Étape 6 — Tester l'authentification

```bash
# Récupérer un token admin
TOKEN=$(curl -s -X POST \
  'http://localhost:8080/auth/realms/wasel/protocol/openid-connect/token' \
  -d 'client_id=wasel-api&username=admin@wasel.ma&password=admin123&grant_type=password' \
  | grep -o '"access_token":"[^"]*' | cut -d'"' -f4)

# Tester une route protégée
curl -X GET http://localhost/api/auth/me \
  -H "Authorization: Bearer $TOKEN"
```

### Comptes de test disponibles

| Email | Mot de passe | Rôle |
|---|---|---|
| `admin@wasel.ma` | `admin123` | ADMIN |
| `client@wasel.ma` | `client123` | CLIENT |

---

## Étape 7 — Lancer la suite de tests automatique

```bash
bash scripts/test-auth.sh
# Résultat attendu : Total Passed: 17 / Total Failed: 0
```

---

## Workflow Git

```bash
# Toujours partir de dev à jour
git checkout dev
git pull origin dev

# Créer ta branche
git checkout -b feature/ma-feature

# Développer, tester, commiter
dotnet build backend/Wasel.Api/Wasel.Api.csproj
git add .
git commit -m "Add my feature"
git push origin feature/ma-feature

# Créer une Pull Request sur GitHub vers 'dev'
```

---

## Structure du code à connaître

```
backend/Wasel.Api/
├── Modules/Auth/        ← JWT, endpoints /api/auth/*
│   ├── Controllers/     ← Reçoit les requêtes HTTP
│   ├── Services/        ← Logique métier
│   └── DTOs/            ← Objets de transfert (requête/réponse)
├── Modules/Users/       ← Profils locaux PostgreSQL
├── Shared/Security/     ← CurrentUserService, claims JWT
└── Program.cs           ← Configuration générale
```

**Règle principale :** Chaque module est indépendant. Ne jamais accéder directement aux tables d'un autre module.

---

## Commandes utiles au quotidien

```bash
# Compiler
dotnet build backend/Wasel.Api/Wasel.Api.csproj

# Voir les logs d'un service
docker compose logs -f wasel-api
docker compose logs -f wasel-keycloak

# Redémarrer un service
docker compose restart wasel-api

# Réinitialiser complètement (supprime les données !)
docker compose down -v && docker compose up -d --build

# Ajouter une migration EF Core
cd backend/Wasel.Api
dotnet ef migrations add "NomDeLaMigration"
```

---

## Ressources

| Document | Lien |
|---|---|
| Architecture backend | [docs/backend-guide.md](./backend-guide.md) |
| Intégration auth frontend | [docs/frontend-auth-guide.md](./frontend-auth-guide.md) |
| Référence API | [docs/api-reference.md](./api-reference.md) |
| Documentation interactive | `http://localhost:5000/scalar/v1` |
