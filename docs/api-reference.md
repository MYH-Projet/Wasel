# Référence API — Wasel Backend

> Base URL directe : `http://localhost:5000`
> Base URL via Nginx : `http://localhost`

---

## Authentification

Toutes les routes protégées requièrent un header :
```
Authorization: Bearer <access_token>
```

### Obtenir un token (test local uniquement)

```bash
curl -X POST 'http://localhost:8080/auth/realms/wasel/protocol/openid-connect/token' \
  -H 'Content-Type: application/x-www-form-urlencoded' \
  -d 'client_id=wasel-api&username=admin@wasel.ma&password=admin123&grant_type=password'
```

**Réponse :**
```json
{
  "access_token": "eyJhbGciOiJSUzI1NiIs...",
  "expires_in": 300,
  "token_type": "Bearer"
}
```

---

## 1. Health Check

### `GET /api/health`

Vérifie que l'API est opérationnelle. **Aucun token requis.**

**Réponse 200 :**
```json
{
  "status": "Healthy",
  "service": "Wasel.Api",
  "timestamp": "2026-05-06T13:00:00Z"
}
```

---

## 2. Authentification & Profil

### `GET /api/auth/me`

Retourne le profil de l'utilisateur connecté. **Crée automatiquement le profil local PostgreSQL** si l'utilisateur n'existe pas encore (auto-sync).

**Headers :** `Authorization: Bearer <token>`

**Réponse 200 :**
```json
{
  "keycloakId": "uuid-keycloak",
  "localUserId": "uuid-postgres",
  "email": "admin@wasel.ma",
  "firstName": "Admin",
  "lastName": "Wassel",
  "roles": ["ADMIN"],
  "status": "Active",
  "cin": null,
  "phone": null
}
```

**Codes d'erreur :**
| Code | Cause |
|---|---|
| `401` | Token absent, invalide ou expiré |

---

### `PATCH /api/auth/me/profile`

Met à jour le profil métier local. **Crée automatiquement le profil local** si absent.

**Headers :** `Authorization: Bearer <token>`, `Content-Type: application/json`

**Body :**
```json
{
  "cin": "AB123456",
  "phone": "+212600000000",
  "firstName": "Yassine",
  "lastName": "Amrani",
  "profileObjectKey": null
}
```
*Tous les champs sont optionnels — seuls les champs fournis sont mis à jour.*

**Réponse 200 :** Profil mis à jour (même format que `/api/auth/me`)

**Codes d'erreur :**
| Code | Cause |
|---|---|
| `401` | Token invalide |
| `400` | Données invalides |

---

### `POST /api/auth/sync`

Force la synchronisation du profil Keycloak → PostgreSQL.
> **⚠️ Optionnel** — `/api/auth/me` fait déjà l'auto-sync. Conservé pour la compatibilité.

**Headers :** `Authorization: Bearer <token>`

**Réponse 200 :** Profil synchronisé

---

### `GET /api/auth/claims` *(Dev only)*

Retourne les claims bruts du JWT. Désactivé en production.

**Réponse 200 :**
```json
{
  "sub": "uuid-keycloak",
  "email": "admin@wasel.ma",
  "realm_access": {
    "roles": ["ADMIN"]
  }
}
```

---

## 3. Administration

> Tous ces endpoints requièrent le rôle **ADMIN**.

### `GET /api/admin/users`

Liste tous les utilisateurs locaux.

**Réponse 200 :**
```json
[
  {
    "id": "uuid",
    "email": "client@wasel.ma",
    "firstName": "Client",
    "lastName": "Test",
    "status": "Pending",
    "createdAt": "2026-05-06T12:00:00Z"
  }
]
```

---

### `GET /api/admin/users/{id}`

Détail d'un utilisateur spécifique.

**Paramètres :**
- `id` : UUID de l'utilisateur local (PostgreSQL)

**Réponse 200 :** Objet utilisateur complet

**Codes d'erreur :**
| Code | Cause |
|---|---|
| `403` | Rôle insuffisant |
| `404` | Utilisateur non trouvé |

---

### `PATCH /api/admin/users/{id}/status`

Change le statut d'approbation d'un utilisateur.

**Body :**
```json
{
  "status": 1
}
```

**Valeurs de status :**
| Valeur | Signification |
|---|---|
| `0` | Pending (en attente) |
| `1` | Active |
| `2` | Inactive |
| `3` | Blocked |

**Réponse 200 :** Utilisateur mis à jour

---

## 4. Codes de retour globaux

| Code | Signification | Action côté client |
|---|---|---|
| `200` | OK | Afficher les données |
| `400` | Données invalides | Afficher le message d'erreur |
| `401` | Token absent / invalide / expiré | Rediriger vers login Keycloak |
| `403` | Rôle insuffisant | Afficher "Accès refusé" |
| `404` | Ressource non trouvée | Gérer l'absence de données |
| `500` | Erreur serveur | Message générique d'erreur |
