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
Le workflow GitHub Actions s'attend à trouver le dossier `~/wasel`.
```bash
mkdir -p ~/wasel/infra/nginx
mkdir -p ~/wasel/infra/keycloak
```

---

## 2. Fichiers à placer manuellement sur le VPS (La première fois)

GitHub Actions se chargera de mettre à jour les images, mais vous devez copier les fichiers de configuration manuellement la première fois.

**Depuis votre machine locale**, utilisez `scp` ou FileZilla pour copier ces fichiers vers le VPS dans le dossier `~/wasel` :
- `docker-compose.staging.yml`
- `infra/nginx/staging.conf`
- `infra/keycloak/realm-export.json`
- `.env.staging.example`

### Création du fichier `.env.staging` (Très Important)
Sur le VPS :
```bash
cd ~/wasel
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
cd ~/wasel
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
