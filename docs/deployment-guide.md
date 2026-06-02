# Guide de Déploiement : Environnement de Staging (VPS)

Ce document explique comment configurer l'environnement de "Staging" (pré-production) sur un serveur virtuel privé (VPS), par exemple sur DigitalOcean, en utilisant Docker et GitHub Actions.

## 1. Prérequis sur le VPS (DigitalOcean)

Avant de pouvoir déployer, vous devez préparer votre VPS.
Connectez-vous à votre VPS via SSH en tant que `deploy` :
```bash
ssh deploy@<IP_DU_VPS>
```

### Installation de Docker et Docker Compose
```bash
# Installer Docker
sudo apt update
sudo apt install -y docker.io docker-compose-v2

# Activer Docker au démarrage
sudo systemctl enable --now docker
```

### Création du dossier de l'application
Le workflow GitHub Actions s'attend à trouver le dossier `/opt/wasel-staging`.
```bash
sudo mkdir -p /opt/wasel-staging/infra/nginx
sudo mkdir -p /opt/wasel-staging/infra/keycloak
sudo chown -R deploy:deploy /opt/wasel-staging
```

---

## 2. Fichiers synchronisés et configuration initiale

Le workflow GitHub Actions synchronise automatiquement ces fichiers vers le VPS :
- `docker-compose.staging.yml`
- `.env.staging.example`
- `infra/nginx/staging.conf`
- `infra/keycloak/realm-import.json`

La première fois, vous devez seulement créer le fichier d'environnement à partir du modèle :

### Création du fichier `.env.staging` (Très Important)
Sur le VPS :
```bash
cd /opt/wasel-staging
cp .env.staging.example .env.staging
```
Ouvrez `.env.staging` avec `nano` :
```bash
nano .env.staging
```
**Modifiez impérativement** les mots de passe (PostgreSQL, MinIO, Keycloak) par des mots de passe sécurisés.
> *Note: Ce fichier `.env.staging` ne doit jamais être commité sur GitHub pour des raisons de sécurité.*

---

## 3. Configuration des Secrets GitHub (CI/CD)

Pour que les workflows GitHub Actions (`build.yml` et `deploy.yml`) fonctionnent, vous devez configurer les **Secrets** dans votre dépôt GitHub.

Allez sur GitHub → Votre dépôt → **Settings** → **Secrets and variables** → **Actions** → Cliquez sur **New repository secret**.

### Liste des Secrets à ajouter :

1. **DOCKERHUB_USERNAME** : `myelhadri` *(Tu l'as déjà fait !)*
2. **DOCKERHUB_TOKEN** : Ton token d'accès Docker Hub *(Tu l'as déjà fait !)*
3. **STAGING_HOST** : L'adresse IP publique de ton VPS (ex: `167.71.x.x`).
4. **STAGING_USER** : L'utilisateur SSH pour se connecter au VPS (doit être `deploy` et faire partie du groupe docker).
5. **STAGING_SSH_KEY** : Ta clé SSH privée complète.

### Comment obtenir et configurer `STAGING_SSH_KEY` ?
C'est la clé privée qui permet à GitHub de se connecter au VPS.

1. **Sur ton ordinateur local** (ou sur le VPS), génère une clé SSH dédiée :
   ```bash
   ssh-keygen -t rsa -b 4096 -C "github-actions"
   ```
   (Ne mettez pas de mot de passe à cette clé)
2. Copiez le contenu de la clé publique (`id_rsa.pub`) et ajoutez-la sur le VPS dans le fichier `~/.ssh/authorized_keys` de l'utilisateur `STAGING_USER`.
3. Copiez le contenu exact (incluant `-----BEGIN...` et `-----END...`) de la clé privée (`id_rsa`) et collez-le dans le secret GitHub **STAGING_SSH_KEY**.

---

## 4. Les 3 Workflows CI/CD

L'architecture est composée de 3 fichiers dans `.github/workflows/` :

1. **`ci.yml` (Tests Backend)**
   - **Déclencheur** : `pull_request` vers `dev`
   - **Rôle** : S'assure que le code compile et que les tests unitaires passent. Bloque le merge en cas d'erreur.

2. **`build.yml` (Build & Push)**
   - **Déclencheur** : `push` vers `main` (quand `dev` est mergé dans `main`)
   - **Rôle** : Compile les nouvelles images Docker (backend et frontend) et les envoie sur Docker Hub (`myelhadri/wasel-api` et `myelhadri/wasel-frontend`).

3. **`deploy.yml` (Déploiement Staging)**
   - **Déclencheur** : `workflow_dispatch` (Déclenchement Manuel via l'onglet "Actions" sur GitHub)
   - **Rôle** : Se connecte au VPS en SSH, télécharge la dernière image Docker Hub, et relance les conteneurs (`docker compose up -d`).

---

## 5. Comment lancer un déploiement ?

1. Développez sur votre branche.
2. Créez une Pull Request vers `dev`. GitHub lance les tests (`ci.yml`).
3. Mergez la PR dans `dev`.
4. Quand vous êtes prêt à publier, mergez `dev` dans `main`. GitHub va construire et pousser les images (`build.yml`).
5. Allez dans l'onglet **Actions** de GitHub.
6. Cliquez sur le workflow **Deploy to Staging VPS**.
7. Cliquez sur **Run workflow**. 
8. GitHub se connecte à votre VPS et déploie la nouvelle version automatiquement !

---

## 6. Maintenance & Rollback

### Voir les logs sur le VPS :
```bash
cd /opt/wasel-staging
docker compose -f docker-compose.staging.yml logs -f wasel-api
```

### Rollback en cas de problème
Si la nouvelle version plante, connectez-vous au VPS et modifiez `docker-compose.staging.yml` pour pointer vers le tag spécifique de la version précédente (ex: `image: myelhadri/wasel-api:abc1234`), puis relancez :
```bash
docker compose -f docker-compose.staging.yml up -d
```

---

## 7. Gestion du Thème Custom Keycloak

Le projet inclut un thème personnalisé pour Keycloak (`my-theme`), qui surcharge notamment la page de connexion (`login`).

### Fonctionnement Local
En développement, le thème est monté dynamiquement via un volume dans `docker-compose.yml`. Cela permet de voir les modifications en temps réel sur les fichiers `.ftl` ou CSS sans avoir à reconstruire d'image.
Cependant, l'image locale est configurée pour utiliser un contexte de build personnalisé si nécessaire via :
```bash
docker compose up -d --build wasel-keycloak
```

### Staging et Production
En staging ou production, le thème DOIT être packagé au sein d'une image Docker personnalisée. Le fichier `infra/keycloak/Dockerfile` a été créé à cet effet.

Le workflow `.github/workflows/keycloak-docker.yml` se charge de builder et pousser cette image sur Docker Hub automatiquement lors d'une modification du thème sur la branche `main`.

Le déploiement staging utilisera les variables suivantes (définies dans `.env.staging`) :
- `KEYCLOAK_IMAGE=${DOCKERHUB_USERNAME}/wasel-keycloak:latest`

Le realm Wassel configure par défaut `"loginTheme": "my-theme"`. Les thèmes `account` et `email` utiliseront le thème de base tant qu'ils ne sont pas explicitement créés.

### Commandes utiles pour test manuel de l'image Keycloak
```bash

### Création du fichier `.env.staging` (Très Important)
Sur le VPS :
```bash
cd /opt/wasel-staging
cp .env.staging.example .env.staging
```
Ouvrez `.env.staging` avec `nano` :
```bash
nano .env.staging
```
**Modifiez impérativement** les mots de passe (PostgreSQL, MinIO, Keycloak) par des mots de passe sécurisés.
> *Note: Ce fichier `.env.staging` ne doit jamais être commité sur GitHub pour des raisons de sécurité.*

---

## 3. Configuration des Secrets GitHub (CI/CD)

Pour que les workflows GitHub Actions (`build.yml` et `deploy.yml`) fonctionnent, vous devez configurer les **Secrets** dans votre dépôt GitHub.

Allez sur GitHub → Votre dépôt → **Settings** → **Secrets and variables** → **Actions** → Cliquez sur **New repository secret**.

### Liste des Secrets à ajouter :

1. **DOCKERHUB_USERNAME** : `myelhadri` *(Tu l'as déjà fait !)*
2. **DOCKERHUB_TOKEN** : Ton token d'accès Docker Hub *(Tu l'as déjà fait !)*
3. **STAGING_HOST** : L'adresse IP publique de ton VPS (ex: `167.71.x.x`).
4. **STAGING_USER** : L'utilisateur SSH pour se connecter au VPS (doit être `deploy` et faire partie du groupe docker).
5. **STAGING_SSH_KEY** : Ta clé SSH privée complète.

### Comment obtenir et configurer `STAGING_SSH_KEY` ?
C'est la clé privée qui permet à GitHub de se connecter au VPS.

1. **Sur ton ordinateur local** (ou sur le VPS), génère une clé SSH dédiée :
   ```bash
   ssh-keygen -t rsa -b 4096 -C "github-actions"
   ```
   (Ne mettez pas de mot de passe à cette clé)
2. Copiez le contenu de la clé publique (`id_rsa.pub`) et ajoutez-la sur le VPS dans le fichier `~/.ssh/authorized_keys` de l'utilisateur `STAGING_USER`.
3. Copiez le contenu exact (incluant `-----BEGIN...` et `-----END...`) de la clé privée (`id_rsa`) et collez-le dans le secret GitHub **STAGING_SSH_KEY**.

---

## 4. Les 3 Workflows CI/CD

L'architecture est composée de 3 fichiers dans `.github/workflows/` :

1. **`ci.yml` (Tests Backend)**
   - **Déclencheur** : `pull_request` vers `dev`
   - **Rôle** : S'assure que le code compile et que les tests unitaires passent. Bloque le merge en cas d'erreur.

2. **`build.yml` (Build & Push)**
   - **Déclencheur** : `push` vers `main` (quand `dev` est mergé dans `main`)
   - **Rôle** : Compile les nouvelles images Docker (backend et frontend) et les envoie sur Docker Hub (`myelhadri/wasel-api` et `myelhadri/wasel-frontend`).

3. **`deploy.yml` (Déploiement Staging)**
   - **Déclencheur** : `workflow_dispatch` (Déclenchement Manuel via l'onglet "Actions" sur GitHub)
   - **Rôle** : Se connecte au VPS en SSH, télécharge la dernière image Docker Hub, et relance les conteneurs (`docker compose up -d`).

---

## 5. Comment lancer un déploiement ?

1. Développez sur votre branche.
2. Créez une Pull Request vers `dev`. GitHub lance les tests (`ci.yml`).
3. Mergez la PR dans `dev`.
4. Quand vous êtes prêt à publier, mergez `dev` dans `main`. GitHub va construire et pousser les images (`build.yml`).
5. Allez dans l'onglet **Actions** de GitHub.
6. Cliquez sur le workflow **Deploy to Staging VPS**.
7. Cliquez sur **Run workflow**. 
8. GitHub se connecte à votre VPS et déploie la nouvelle version automatiquement !

---

## 6. Maintenance & Rollback

### Voir les logs sur le VPS :
```bash
cd /opt/wasel-staging
docker compose -f docker-compose.staging.yml logs -f wasel-api
```

### Rollback en cas de problème
Si la nouvelle version plante, connectez-vous au VPS et modifiez `docker-compose.staging.yml` pour pointer vers le tag spécifique de la version précédente (ex: `image: myelhadri/wasel-api:abc1234`), puis relancez :
```bash
docker compose -f docker-compose.staging.yml up -d
```

---

## 7. Gestion du Thème Custom Keycloak

Le projet inclut un thème personnalisé pour Keycloak (`my-theme`), qui surcharge notamment la page de connexion (`login`).

### Fonctionnement Local
En développement, le thème est monté dynamiquement via un volume dans `docker-compose.yml`. Cela permet de voir les modifications en temps réel sur les fichiers `.ftl` ou CSS sans avoir à reconstruire d'image.
Cependant, l'image locale est configurée pour utiliser un contexte de build personnalisé si nécessaire via :
```bash
docker compose up -d --build wasel-keycloak
```

### Staging et Production
En staging ou production, le thème DOIT être packagé au sein d'une image Docker personnalisée. Le fichier `infra/keycloak/Dockerfile` a été créé à cet effet.

Le workflow `.github/workflows/keycloak-docker.yml` se charge de builder et pousser cette image sur Docker Hub automatiquement lors d'une modification du thème sur la branche `main`.

Le déploiement staging utilisera les variables suivantes (définies dans `.env.staging`) :
- `KEYCLOAK_IMAGE=${DOCKERHUB_USERNAME}/wasel-keycloak:latest`

Le realm Wassel configure par défaut `"loginTheme": "my-theme"`. Les thèmes `account` et `email` utiliseront le thème de base tant qu'ils ne sont pas explicitement créés.

### Commandes utiles pour test manuel de l'image Keycloak
```bash
# Build local de l'image custom
docker build -t wasel-keycloak-test ./infra/keycloak

# Démarrer le conteneur en local via docker-compose
docker compose up -d --build wasel-keycloak

# Tag et Push (Normalement géré par GitHub Actions)
docker tag wasel-keycloak-test dockerhub-username/wasel-keycloak:latest
docker push dockerhub-username/wasel-keycloak:latest
```

---

## 8. Staging Keycloak + Nginx standard configuration

Afin de garantir que l'authentification Keycloak fonctionne correctement sur l'environnement de staging, respectez rigoureusement ces règles :

### 1. Configuration de Keycloak (`docker-compose.staging.yml` et `.env.staging`)
- **`STAGING_DOMAIN` doit contenir seulement le domaine**, sans `https://` et sans `/auth`.
- **`KC_HOSTNAME` doit prendre uniquement ce domaine (`${STAGING_DOMAIN}`)**, et SURTOUT PAS le préfixe `https://`. Keycloak 24+ le rajoute automatiquement. Mettre `https://` produira des URLs de formulaire invalides du type `https://https//...` bloquant le login.
- **`KC_HTTP_RELATIVE_PATH=/auth`** doit être utilisé. C'est cette variable qui instruit Keycloak de s'exposer derrière le chemin `/auth`.

**Exemple correct :**
```yaml
# Dans .env.staging
STAGING_DOMAIN=staging.lwasel.tech

# Dans docker-compose.staging.yml
KC_HOSTNAME=${STAGING_DOMAIN}
KC_HTTP_RELATIVE_PATH=/auth
```

**Exemples ❌ INTERDITS ❌ :**
```yaml
STAGING_DOMAIN=https://staging.lwasel.tech
STAGING_DOMAIN=staging.lwasel.tech/auth
KC_HOSTNAME=https://${STAGING_DOMAIN}
KC_HOSTNAME=https://${STAGING_DOMAIN}/auth
```

### 2. Configuration de Nginx (`staging.conf`)
Nginx doit relayer `/auth` **sans trailing slash dans l'upstream**.
- **Correct :** `proxy_pass http://wasel-keycloak-staging:8080;` (ou `http://wasel_keycloak;`)
- **Faux :** `proxy_pass http://wasel-keycloak-staging:8080/auth/;` (cela doublerait le chemin `/auth/auth`).

### 3. Fichier Realm (Source de vérité)
Le seul fichier qui doit être utilisé pour configurer le realm est **`infra/keycloak/realm-import.json`**.
- Assurez-vous de ne jamais monter `realm-export.json` s'il n'est pas la version finale et complète.
- Vérifiez toujours que le client `wasel-front` est bien présent dans `realm-import.json`.

### 4. Certificats SSL
Assurez-vous que le certificat Let's Encrypt est bien présent et monté pour que le reverse proxy fonctionne :
```bash
docker run --rm -v wasel-staging_letsencrypt-staging:/etc/letsencrypt alpine ls -la /etc/letsencrypt/live/staging.lwasel.tech
```

### 5. Diagnostics et Tests
Pour tester si le login fonctionne :
```bash
curl -k -I "https://staging.lwasel.tech/auth/realms/wasel/protocol/openid-connect/auth?client_id=wasel-front&response_type=code&redirect_uri=https%3A%2F%2Fstaging.lwasel.tech%2Fendpoint%2Fcallback&scope=openid"
```
Un retour `HTTP/1.1 200 OK` avec `curl -s -o /dev/null -w '%{http_code}' ...` indique que la configuration est fonctionnelle.
Si la page renvoie `400 Bad Request` avec `Client not found`, vérifiez le fichier `realm-import.json`.

### ⚠️ Ce qu'il ne faut PAS faire
- Ne pas utiliser un fichier `realm-export.json` autogénéré s'il manque des clients (`wasel-front`).
- Ne jamais supprimer le volume de base de données de Keycloak (`wasel-keycloak-data-staging`) sans faire de backup au préalable, sinon les mots de passe des utilisateurs existants seront perdus.
- Ne pas faire de modifications manuelles (comme des fichiers de configuration ou le `docker-compose.staging.yml`) directement sur le VPS sans les commiter d'abord sur Git. Git reste l'unique source de vérité.

---

## 9. Frontend Auth URL on Staging

La configuration du Frontend Astro sur l'environnement de staging obéit à des règles strictes pour garantir que la redirection Keycloak fonctionne correctement :

1. **Le frontend staging ne doit jamais générer `localhost`** : Les variables d'environnement comme `PUBLIC_APP_URL` et `PUBLIC_KEYCLOAK_URL` ne doivent jamais retomber sur `http://localhost:8000` lors du runtime sur le VPS. Le code utilise désormais `Astro.url.origin` pour s'adapter dynamiquement au domaine (ex: `https://staging.lwasel.tech`).
2. **`PUBLIC_KEYCLOAK_URL` doit pointer vers staging** : Dans `docker-compose.staging.yml`, cette variable peut être définie à `/auth`. Le backend SSR Astro et le client React calculeront l'URL absolue correctement.
3. **`redirect_uri` staging doit pointer vers staging** : Lors de la soumission du formulaire de connexion ou de la redirection vers Keycloak, l'URL de retour (`redirect_uri`) générée dans le code doit impérativement être `https://staging.lwasel.tech/endpoint/callback` (encodée).
4. **`localhost` est réservé au développement local** : Ce domaine ne doit apparaître que lorsque vous lancez l'application sur votre propre machine via `npm run dev`.

### Comment tester que l'image frontend ne contient pas `localhost` en dur ?
Vous pouvez utiliser `grep` directement à l'intérieur du conteneur frontend sur le VPS pour vérifier que `localhost` n'a pas fuité dans le build de production :

```bash
docker exec wasel-frontend-staging sh -c "grep -R 'localhost:8000' -n /app 2>/dev/null || true"
```
Si cette commande retourne des occurrences dans les fichiers JavaScript générés (dans `/app/dist`), c'est que l'image a été mal construite et qu'elle provoquera des erreurs 502 Bad Gateway.
