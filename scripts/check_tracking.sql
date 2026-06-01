-- ══════════════════════════════════════════════════
-- Wasel — Vérification des points GPS enregistrés
-- ══════════════════════════════════════════════════
-- Usage:
--   docker cp scripts/check_tracking.sql wasel-postgres:/tmp/check.sql
--   docker exec wasel-postgres psql -U wasel_user -d wasel_db -f /tmp/check.sql

-- 1. Dernières 10 positions GPS enregistrées
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

-- 2. Nombre total de points enregistrés
SELECT COUNT(*) AS total_tracking_points FROM tracking_points;

-- 3. Résumé par driver
SELECT
    tp."DriverId",
    COUNT(*) AS nb_points,
    MIN(tp."RecordedAt") AS premiere_position,
    MAX(tp."RecordedAt") AS derniere_position
FROM tracking_points tp
GROUP BY tp."DriverId";
