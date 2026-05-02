# Configuration de Keycloak pour Wassel

Keycloak est notre système de gestion des identités et des accès (IAM).
Pour l'instant, la configuration se fait manuellement. Ce guide vous montre comment préparer Keycloak pour tester l'API localement.

## 1. Accéder à Keycloak

1. Assurez-vous que l'infrastructure Docker est lancée (`docker compose up -d`).
2. Ouvrez [http://localhost:8080](http://localhost:8080) dans votre navigateur.
3. Cliquez sur **Administration Console**.
4. Connectez-vous avec :
   - Username: `admin`
   - Password: `admin`

## 2. Créer le Realm "wasel"

1. En haut à gauche, survolez "Master" et cliquez sur le bouton **Create Realm**.
2. **Realm name** : `wasel`
3. Cliquez sur **Create**.

## 3. Créer le Client "wasel-api"

1. Dans le menu de gauche, allez dans **Clients** puis cliquez sur **Create client**.
2. **Client type** : `OpenID Connect`
3. **Client ID** : `wasel-api`
4. Cliquez sur **Next**.
5. **Client authentication** : OFF (Laissez tel quel. Le client est public, car mobile/web feront le login directement).
6. **Authorization** : OFF
7. **Standard flow** : ON
8. **Direct access grants** : ON
9. Cliquez sur **Next**.
10. **Valid redirect URIs** : `http://localhost:*` (Ou l'URL exacte de votre app Flutter/Astro)
11. **Web origins** : `+` (Permet toutes les origines CORS pour les tests)
12. Cliquez sur **Save**.

## 4. Créer les Rôles de Realm

1. Allez dans **Realm roles**.
2. Cliquez sur **Create role**.
3. Créez les trois rôles suivants un par un :
   - `ADMIN`
   - `DRIVER`
   - `CLIENT`

## 5. Créer des utilisateurs de test

1. Allez dans **Users** et cliquez sur **Add user**.
2. Remplissez obligatoirement ces champs (sinon l'API renverra une erreur "Account is not fully set up") :
   - **Username** : `admin@wasel.ma`
   - **Email** : `admin@wasel.ma`
   - **First name** : `Admin`
   - **Last name** : `Wassel`
   - **Email verified** : `ON`
3. Vérifiez bien que le champ **Required user actions** est complètement VIDE. S'il contient "Update Password" ou "Update Profile", retirez-les.
4. Cliquez sur **Create**.
5. Allez dans l'onglet **Credentials**.
6. Cliquez sur **Set password**.
7. Mettez un mot de passe simple (ex: `admin123`).
8. **TRÈS IMPORTANT** : Décochez **Temporary** pour que le mot de passe soit définitif.
9. Cliquez sur **Save** puis sur **Save password** dans la popup de confirmation.
10. Allez dans l'onglet **Role mapping**.
11. Cliquez sur **Assign role**, filtrez par `ADMIN`, sélectionnez-le et validez.

Répétez la procédure pour créer un utilisateur `client@wasel.ma` (rôle `CLIENT`, Prénom: `Client`, Nom: `Test`).

## 6. Tester l'API avec un Token

Pour appeler l'API, vous avez besoin d'un Access Token JWT.
Vous pouvez l'obtenir via `curl` ou Postman (Resource Owner Password Grant) :

```bash
curl --location --request POST 'http://localhost:8080/realms/wasel/protocol/openid-connect/token' \
--header 'Content-Type: application/x-www-form-urlencoded' \
--data-urlencode 'client_id=wasel-api' \
--data-urlencode 'username=admin@wasel.ma' \
--data-urlencode 'password=admin123' \
--data-urlencode 'grant_type=password'
```

Copiez la valeur de `access_token` dans la réponse.

Testez l'API :

```bash
curl -i http://localhost:5000/api/auth/me \
  -H "Authorization: Bearer VOTRE_TOKEN_ICI"
```

## 7. Tests automatiques Auth

Un script de test bout-en-bout a été créé pour valider automatiquement toute l'intégration Auth.

### Pré-requis :
1. Les services Docker doivent tourner (`docker compose up -d`).
2. Le Realm `wasel` doit être configuré.
3. Les utilisateurs de test `admin@wasel.ma` et `client@wasel.ma` doivent exister (voir étape 5).

### Lancer les tests :
Depuis Git Bash, Linux, ou macOS à la racine du projet :

```bash
bash scripts/test-auth.sh
```

Le script vous affichera un résumé avec le nombre de tests PASS/FAIL.
