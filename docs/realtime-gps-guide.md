# Guide Intégration SignalR - Suivi GPS Temps Réel

Le projet Wasel utilise ASP.NET Core SignalR pour le suivi GPS en temps réel des livreurs.
La position est sauvegardée dans PostgreSQL sous forme d'historique continu (`TrackingPoint`) avec un contrôle de fréquence, et est diffusée simultanément aux clients abonnés en temps réel.

## 1. Endpoints et Routes

- **Hub WebSocket** : `wss://localhost/api/hubs/gps` (ou en `ws://` selon l'environnement).
- **REST** : `GET /api/tracking/deliveries/{deliveryId}/last-position`
- **REST** : `GET /api/tracking/drivers/{driverId}/last-position`
- **REST** : `GET /api/tracking/me/last-position`

## 2. Authentification SignalR

Contrairement aux requêtes HTTP classiques, les clients WebSocket (notamment Flutter/JS) envoient souvent le jeton JWT dans l'URL.
Le backend est configuré pour lire ce token depuis la querystring **uniquement** sur la route `/api/hubs/gps`.

**Format d'appel :**
`ws://localhost/api/hubs/gps?access_token=eyJhbG...`

## 3. Rôles et Autorisations

- **DRIVER** : 
  - Peut appeler la méthode Hub `SendPosition`.
  - Son statut dans la table locale `Drivers` doit être `Approved`.
  - Le backend lie automatiquement le Driver à l'utilisateur connecté via `EnsureCurrentUserExistsAsync`.
- **CLIENT** :
  - Peut écouter les positions d'une de ses courses actives via `JoinDeliveryGroup`.
  - Doit être le propriétaire de la course (`Delivery.ClientId`).
- **ADMIN** :
  - Peut tout écouter.

## 4. Méthodes du Hub SignalR

### A. Côté Client Mobile Livreur (Envoi)

```javascript
// Connexion
const connection = new signalR.HubConnectionBuilder()
    .withUrl("/api/hubs/gps?access_token=" + token)
    .build();

await connection.start();

// Envoi de la position
await connection.invoke("SendPosition", {
    Latitude: 33.589,
    Longitude: -7.603,
    DeliveryId: "123e4567-e89b-12d3-a456-426614174000", // Optionnel
    Speed: 15.5, // Optionnel
    Heading: 90.0 // Optionnel
});
```

### B. Côté Client Utilisateur (Écoute)

```javascript
const connection = new signalR.HubConnectionBuilder()
    .withUrl("/api/hubs/gps?access_token=" + token)
    .build();

// S'abonner aux mises à jour
connection.on("ReceivePosition", (position) => {
    console.log("Nouvelle position du livreur :", position.latitude, position.longitude);
});

await connection.start();

// Rejoindre le groupe spécifique à la livraison
await connection.invoke("JoinDeliveryGroup", "123e4567-e89b-12d3-a456-426614174000");

// Optionnel: quitter le groupe
// await connection.invoke("LeaveDeliveryGroup", "123e4567-e89b-12d3-a456-426614174000");
```

## 5. Architecture & Persistance (Important)

L'approche adoptée est un **historique contrôlé** (Append-only `TrackingPoint`).

- **Throttle (Limitation de fréquence DB)** :
  Pour éviter de saturer la base de données, l'API utilise actuellement `IMemoryCache` pour ne persister la position d'un livreur dans PostgreSQL que toutes les **5 secondes au minimum** (ou lors d'un changement de livraison).
  Toutes les positions reçues sont cependant **diffusées instantanément** sur le Hub SignalR, garantissant un temps réel parfait côté Client.

- **Scale-out et Redis** :
  Le `IMemoryCache` fonctionne car nous sommes actuellement sur une seule instance backend (Sprint 1). Si le backend doit être mis à l'échelle (scale-out horizontal sur plusieurs instances), ce throttle en mémoire devra être migré vers **Redis** pour partager l'état du throttle entre les instances.

- **Purge et Archivage** :
  L'application ne vise pas à maintenir un **historique illimité** actif en base de données de production.
  La table `tracking_points` va croître rapidement. Une tâche planifiée future devra être mise en place pour purger ou archiver (Data Lake, stockage froid) les positions GPS antérieures à une certaine durée de rétention (ex: 30 jours).

## 6. Nginx

Nginx a été configuré avec les directives `Upgrade` et `Connection` pour `/api/` :
```nginx
map $http_upgrade $connection_upgrade {
    default upgrade;
    ''      close;
}

location /api/ {
    # ...
    proxy_http_version 1.1;
    proxy_set_header Upgrade $http_upgrade;
    proxy_set_header Connection $connection_upgrade;
    proxy_read_timeout 86400;
}
```
