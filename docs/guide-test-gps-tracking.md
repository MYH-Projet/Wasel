# 🛰️ Guide de Test — Module GPS Tracking (Wasel)

> **Auteur** : Équipe Backend Wasel
> **Dernière mise à jour** : 23/05/2026
> **Temps estimé** : ~10 minutes

Ce guide vous accompagne **pas à pas** pour tester le système de suivi GPS en temps réel de Wasel.
Il couvre la connexion WebSocket (SignalR), l'envoi de positions simulées, et la vérification de la persistance en base de données.

---

## 📋 Table des matières

1. [Pré-requis](#1--pré-requis)
2. [Démarrer l'infrastructure](#2--démarrer-linfrastructure)
3. [Obtenir un token JWT](#3--obtenir-un-token-jwt)
4. [Tester avec l'interface web (test-gps.html)](#4--tester-avec-linterface-web)
5. [Tester avec l'API REST (Postman / curl)](#5--tester-avec-lapi-rest)
6. [Vérifier la persistance en base de données](#6--vérifier-la-persistance-en-base-de-données)
7. [Architecture du module](#7--architecture-du-module)
8. [Résolution de problèmes](#8--résolution-de-problèmes)

---

## 1.  Pré-requis

Assurez-vous d'avoir installé :

| Outil          | Version minimale | Vérification                  |
|----------------|-----------------|-------------------------------|
| Docker Desktop | 4.x+            | `docker --version`            |
| Docker Compose | v2+             | `docker compose version`      |
| PowerShell     | 5.1+            | `$PSVersionTable`             |
| Navigateur     | Chrome / Edge   | N'importe quel navigateur moderne |

> ⚠️ **IMPORTANT** : Docker Desktop doit être **lancé et fonctionnel** avant de commencer.

---

## 2. 🚀 Démarrer l'infrastructure

Ouvrez un terminal PowerShell à la racine du projet `Wasel/` :

```powershell
cd C:\chemin\vers\Wasel
docker compose up -d
```

Attendez que **tous les services** soient démarrés. Vérifiez avec :

```powershell
docker compose ps
```

Vous devez voir ces services en état **running** (ou **healthy**) :

| Service           | Port local | Description                    |
|-------------------|-----------|--------------------------------|
| `wasel-api`       | 5000      | API .NET 10 (backend)          |
| `wasel-postgres`  | 5432      | Base de données PostgreSQL     |
| `wasel-redis`     | 6379      | Cache Redis                    |
| `wasel-keycloak`  | 8080      | Serveur d'authentification     |
| `wasel-minio`     | 9000/9001 | Stockage objets (documents)    |
| `wasel-nginx`     | **8000**  | Reverse proxy (point d'entrée) |

> 💡 **Le point d'entrée principal est toujours `http://localhost:8000`**. Toutes les requêtes passent par Nginx.

Si c'est le **premier lancement**, attendez ~30 secondes que Keycloak finisse son initialisation :

```powershell
docker compose logs wasel-api --tail 5
```

Vous devez voir un log du type :
```
Now listening on: http://[::]:8080
Application started.
```

---

## 3. 🔑 Obtenir un token JWT

Le module GPS exige une authentification. Il faut récupérer un **token JWT** depuis Keycloak.

### Comptes de test disponibles

| Username         | Mot de passe | Rôles          | Peut envoyer du GPS ? |
|------------------|-------------|----------------|----------------------|
| `admin@wasel.ma` | `admin`     | ADMIN + DRIVER | ✅ Oui               |
| `driver@wasel.ma`| `driver`    | DRIVER         | ✅ Oui               |
| `client@wasel.ma`| `client`    | CLIENT         | ❌ Non (réception seulement) |

> ⚠️ **Si un compte donne l'erreur "Account is not fully set up"** :
> Allez dans Keycloak Admin (http://localhost:8000/auth/admin → identifiants `admin`/`admin`),
> trouvez l'utilisateur, onglet **Details** → supprimez toutes les **Required User Actions**,
> puis onglet **Credentials** → définissez le mot de passe avec **Temporary = OFF**.

### Méthode PowerShell (recommandée)

Copiez-collez ce bloc dans PowerShell :

```powershell
# ── Choisissez le compte à utiliser ──
$username = "admin@wasel.ma"
$password = "admin"

# ── Récupération du token ──
$body = @{
    grant_type = "password"
    client_id  = "wasel-api"
    username   = $username
    password   = $password
}

$response = Invoke-RestMethod `
    -Uri "http://localhost:8000/auth/realms/wasel/protocol/openid-connect/token" `
    -Method POST `
    -Body $body

# Le token est copié dans le presse-papiers
$response.access_token | Set-Clipboard
Write-Host "✅ Token copié dans le presse-papiers ! (Longueur: $($response.access_token.Length) caractères)"
```

### Méthode curl (Linux / macOS / Git Bash)

```bash
TOKEN=$(curl -s -X POST \
  "http://localhost:8000/auth/realms/wasel/protocol/openid-connect/token" \
  -d "grant_type=password" \
  -d "client_id=wasel-api" \
  -d "username=admin@wasel.ma" \
  -d "password=admin" \
  | python -c "import sys,json; print(json.load(sys.stdin)['access_token'])")

echo "✅ Token récupéré (${#TOKEN} caractères)"
```

> 📝 **Note** : Les tokens JWT expirent après **5 minutes** par défaut.
> Si votre connexion échoue après un moment, générez un nouveau token.

---

## 4. 🧪 Tester avec l'interface web

### Étape 1 — Ouvrir la page de test

Ouvrez le fichier suivant dans votre navigateur :

```
Wasel/scripts/test-gps.html
```

> Double-cliquez dessus ou faites `Clic droit → Ouvrir avec → Chrome`.

### Étape 2 — Se connecter au Hub SignalR

1. **Collez** le token JWT (Ctrl+V) dans le champ **"Jeton JWT (Access Token)"**
2. Cliquez **"Connecter"**
3. ✅ Le statut doit passer à **"Connecté !"** en vert

Dans les logs, vous devez voir :
```
[HH:MM:SS] Tentative de connexion au WebSocket...
[HH:MM:SS] ✅ Connecté au GpsHub avec succès.
```

### Étape 3 — Envoyer une position manuellement

1. Cliquez **"Envoyer une position factice"**
2. ✅ Vous devez recevoir dans les logs :

```
[HH:MM:SS] ➡️ Envoi de la position (Invoke 'SendPosition')...
[HH:MM:SS] 📡 Reçu via Hub: Lat=33.589..., Lng=-7.603... (Persisté en DB: true)
```

> La première position est **toujours persistée** en base (`Persisté en DB: true`).

### Étape 4 — Tester l'envoi automatique et le throttling

1. Cliquez **"Démarrer Envoi Auto (1/sec)"**
2. Observez les logs pendant **10 secondes** :
   - Les positions sont envoyées **chaque seconde** via WebSocket
   - Mais elles ne sont **persistées en DB que toutes les 5 secondes**
   - Vous verrez alterner `Persisté en DB: true` et `Persisté en DB: false`
3. Cliquez **"Stopper Envoi Auto"** pour arrêter

**Exemple de logs attendus** :
```
[13:38:09] 📡 Reçu: Lat=33.589..., Lng=-7.604... (Persisté en DB: true)   ← 1ère position
[13:38:10] 📡 Reçu: Lat=33.590..., Lng=-7.604... (Persisté en DB: false)  ← throttled
[13:38:11] 📡 Reçu: Lat=33.590..., Lng=-7.604... (Persisté en DB: false)  ← throttled
[13:38:12] 📡 Reçu: Lat=33.590..., Lng=-7.604... (Persisté en DB: false)  ← throttled
[13:38:13] 📡 Reçu: Lat=33.590..., Lng=-7.604... (Persisté en DB: false)  ← throttled
[13:38:14] 📡 Reçu: Lat=33.590..., Lng=-7.604... (Persisté en DB: true)   ← 5s écoulées → persisté !
```

> 💡 **Pourquoi ce throttling ?** Envoyer 1 position/sec × 100 drivers = 100 écritures/sec en DB.
> Le throttling réduit la charge à ~20 écritures/sec tout en gardant le temps réel via WebSocket.

### Étape 5 — Tester le suivi de livraison (optionnel)

Si vous avez un **Delivery ID** (UUID d'une livraison en cours) :

1. Saisissez l'UUID dans le champ **"Delivery ID"** de la section envoi
2. Dans un **2ème onglet** : ouvrez la même page, connectez-vous avec un **compte CLIENT**
3. Saisissez le même Delivery ID dans **"Rejoindre un groupe de livraison"**
4. Cliquez **"JoinDeliveryGroup"**
5. ✅ Le client recevra les positions du driver en temps réel !

---

## 5. 📡 Tester avec l'API REST

En plus du WebSocket, des endpoints REST sont disponibles pour consulter les positions.

### Récupérer la dernière position d'un driver

```powershell
$token = (Get-Clipboard)
$driverId = "eded525f-e55c-4c42-adee-3ee4ab25d5cd"  # ← Remplacez par l'ID réel

Invoke-RestMethod `
    -Uri "http://localhost:8000/api/tracking/drivers/$driverId/last-position" `
    -Headers @{ Authorization = "Bearer $token" }
```

### Récupérer ma propre dernière position (driver connecté)

```powershell
$token = (Get-Clipboard)

Invoke-RestMethod `
    -Uri "http://localhost:8000/api/tracking/me/last-position" `
    -Headers @{ Authorization = "Bearer $token" }
```

### Récupérer la position d'une livraison

```powershell
$token = (Get-Clipboard)
$deliveryId = "VOTRE-DELIVERY-UUID"

Invoke-RestMethod `
    -Uri "http://localhost:8000/api/tracking/deliveries/$deliveryId/last-position" `
    -Headers @{ Authorization = "Bearer $token" }
```

### Réponse type

```json
{
    "id": "a1b2c3d4-...",
    "persisted": true,
    "driverId": "eded525f-...",
    "deliveryId": null,
    "latitude": 33.5899,
    "longitude": -7.6038,
    "heading": null,
    "speedKmh": 25,
    "accuracyMeters": 5.0,
    "recordedAt": "2026-05-23T12:38:09Z"
}
```

---

## 6. 🗄️ Vérifier la persistance en base de données

Pour confirmer que les positions sont bien enregistrées en PostgreSQL, créez le fichier
`scripts/check_tracking.sql` :

```sql
-- Dernières positions GPS enregistrées
SELECT
    tp."Id",
    tp."Latitude",
    tp."Longitude",
    tp."SpeedKmh",
    tp."RecordedAt",
    tp."DeliveryId"
FROM tracking_points tp
ORDER BY tp."RecordedAt" DESC
LIMIT 10;

-- Nombre total de points enregistrés
SELECT COUNT(*) AS total_tracking_points FROM tracking_points;
```

Puis exécutez :

```powershell
docker cp scripts/check_tracking.sql wasel-postgres:/tmp/check.sql
docker exec wasel-postgres psql -U wasel_user -d wasel_db -f /tmp/check.sql
```

**Résultat attendu** : Vous devez voir les points GPS avec les coordonnées autour de
Casablanca (Lat ≈ 33.59, Lng ≈ -7.60).

---

## 7. 🏗️ Architecture du module

### Flux de données

```
┌─────────────────────┐
│   App Mobile/Web    │
│  (ou test-gps.html) │
└─────────┬───────────┘
          │ WebSocket + JWT
          ▼
┌─────────────────────┐
│   Nginx (:8000)     │ ← Reverse proxy avec support WebSocket
└─────────┬───────────┘
          │
          ▼
┌─────────────────────────┐
│   GpsHub (SignalR)       │ ← Authentification JWT + dispatch
│                         │
│  SendPosition(dto)      │───► TrackingService
│  JoinDeliveryGroup()    │       │
│  LeaveDeliveryGroup()   │       ▼
└─────────┬───────────────┘  ┌──────────────┐
          │                  │ MemoryCache   │ ← Throttling (5s)
          │                  │ (IMemoryCache)│
          │                  └──────┬───────┘
          │                         │ si shouldPersist = true
          │                         ▼
          │                  ┌──────────────┐
          │                  │ PostgreSQL   │ ← Table: tracking_points
          │                  └──────────────┘
          ▼
  ┌─────────────────────────────────────────────────┐
  │ Clients.Caller → ReceivePosition (confirmation) │
  │ Clients.Group("delivery-{id}") → ReceivePosition│
  └─────────────────────────────────────────────────┘
```

### Fichiers clés du module

```
backend/Wasel.Api/Modules/Tracking/
├── Controllers/
│   └── TrackingController.cs     ← Endpoints REST (GET positions)
├── DTOs/
│   ├── TrackingPointUpdateDto.cs  ← Données envoyées par le driver
│   └── TrackingPointResponseDto.cs← Données retournées
├── Entities/
│   └── TrackingPoint.cs           ← Modèle EF Core (table DB)
├── Hubs/
│   └── GpsHub.cs                  ← Hub SignalR (WebSocket)
├── Repositories/
│   └── TrackingRepository.cs      ← Accès base de données
└── Services/
    ├── ITrackingService.cs
    └── TrackingService.cs         ← Logique métier + throttling
```

### Règles métier

| Règle | Détail |
|-------|--------|
| **Qui peut envoyer ?** | Uniquement un DRIVER avec `Status = Approved` |
| **Throttling DB** | 1 écriture en DB toutes les **5 secondes** par driver |
| **Temps réel** | **Toutes** les positions sont diffusées via WebSocket (même non persistées) |
| **Groupes** | Un CLIENT peut rejoindre le groupe de sa livraison pour recevoir les positions |
| **Sécurité** | JWT obligatoire sur le Hub + vérification des droits dans le service |

---

## 8. 🔧 Résolution de problèmes

### ❌ "Invalid user credentials"

**Cause** : Mauvais mot de passe.

**Solution** :
1. Allez dans Keycloak Admin : http://localhost:8000/auth/admin (login: `admin`/`admin`)
2. Sélectionnez le realm **wasel** (en haut à gauche)
3. Menu **Users** → Cliquez sur l'utilisateur
4. Onglet **Credentials** → **Reset Password** → mot de passe → **Temporary = OFF**

---

### ❌ "Account is not fully set up"

**Cause** : Des actions requises sont en attente sur le compte (ex: "Update Password").

**Solution** :
1. Keycloak Admin → Users → Cliquez sur l'utilisateur
2. Onglet **Details** → Champ **Required User Actions** → Supprimez toutes les actions (×)
3. **Save**

---

### ❌ "WebSocket failed to connect... proxy blocking WebSockets"

**Cause** : Problème d'issuer JWT ou API pas encore démarrée.

**Vérifications** :
1. Vérifiez que l'API est démarrée : `docker logs wasel-api --tail 10`
2. Cherchez une erreur d'issuer dans les logs :
   ```
   IDX10205: Issuer validation failed. Issuer: 'http://localhost:8000/...'
   ```
3. Si c'est le cas, vérifiez dans `docker-compose.yml` :
   ```yaml
   Keycloak__NginxAuthority=http://localhost:8000/auth/realms/wasel
   ```
4. Relancez l'API : `docker compose up -d wasel-api --force-recreate`

---

### ❌ "Vous n'avez pas de profil de livreur associé"

**Cause** : L'utilisateur a le rôle DRIVER dans Keycloak mais pas de profil Driver en base.

**Solution** : Connectez-vous d'abord via l'app frontend pour déclencher la synchronisation
automatique, ou créez le profil manuellement via l'API admin.

---

### ❌ "Votre compte livreur n'est pas actif/approuvé"

**Cause** : Le driver a `Status = Pending` (0), `Rejected` (2) ou `Suspended` (3).

**Solution** : Un admin doit approuver le compte. En développement, vous pouvez le faire en DB :

```sql
-- Vérifier le status actuel
SELECT "Id", "Status" FROM drivers;

-- Approuver (Status = 1 = Approved)
UPDATE drivers SET "Status" = 1 WHERE "Id" = 'VOTRE-DRIVER-UUID';
```

Valeurs possibles : `0 = Pending`, `1 = Approved`, `2 = Rejected`, `3 = Suspended`

---

### ❌ Le token a expiré

**Symptôme** : Vous étiez connecté, puis la connexion échoue après quelques minutes.

**Solution** : Regénérez un token avec la commande PowerShell de l'[étape 3](#3--obtenir-un-token-jwt).

---

## ✅ Checklist de validation

Cochez chaque point pour confirmer que le module fonctionne correctement :

- [ ] `docker compose up -d` — Tous les services sont **running/healthy**
- [ ] Token JWT obtenu avec succès (longueur ≈ 1100 caractères)
- [ ] `test-gps.html` → Connexion WebSocket : **"Connecté !"**
- [ ] Envoi de position manuelle → `Persisté en DB: true`
- [ ] Envoi automatique (1/sec) → Throttling visible (`true` puis `false` puis `true`)
- [ ] API REST `GET /api/tracking/me/last-position` retourne la dernière position
- [ ] Base de données → `tracking_points` contient les enregistrements

---

> **Besoin d'aide ?** Consultez les logs de l'API :
> ```powershell
> docker logs wasel-api --tail 50
> ```
