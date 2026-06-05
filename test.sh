docker compose exec -T wasel-postgres psql -U wasel_user -d wasel_db -t -A -c 'SELECT "Id" FROM users LIMIT 1;'
