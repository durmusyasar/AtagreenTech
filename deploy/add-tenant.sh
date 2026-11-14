#!/usr/bin/env bash
# Docker Compose yığınına bir tenant ekler: master veritabanına kaydeder, ardından tenant-migrator ile tenant'ın
# veritabanını oluşturup migration'larını uygular. Tenant zaten kayıtlıysa yalnızca migration'lar çalışır.
#
#   deploy/add-tenant.sh <subdomain> "<Ad>"      ör. deploy/add-tenant.sh atagreen "Atagreen"
#
# Tenant <subdomain>.omnisync.com, <subdomain>.omnisync.com.tr ve yerelde <subdomain>.omnisync.localhost adresinden
# çözülür. Yığın (postgres servisi) çalışıyor olmalıdır.

set -euo pipefail

cd "$(dirname "$0")/.."

if [[ $# -ne 2 ]]; then
  echo "Kullanım: $0 <subdomain> \"<Ad>\"" >&2
  exit 64
fi

subdomain="$1"
name="$2"

# OmniSync'in tenant tanımlayıcı kuralı: en fazla 63 karakter; küçük harf, rakam ve tire; tireyle başlayıp bitemez.
if [[ ! "$subdomain" =~ ^[a-z0-9]([a-z0-9-]{0,61}[a-z0-9])?$ || "$subdomain" == "www" ]]; then
  echo "Geçersiz subdomain: '$subdomain' (küçük harf, rakam ve tire; en fazla 63 karakter; 'www' olamaz)" >&2
  exit 65
fi

if [[ -z "${name// /}" ]]; then
  echo "Tenant adı boş olamaz." >&2
  exit 65
fi

database="atagreen_stock_${subdomain//-/_}"

# Parola, postgres container'ına bağlanan secret dosyasından SQL içinde okunur; komut satırında görünmez.
id=$(docker compose exec -T postgres psql -U atagreen -d atagreen_stock_master -v ON_ERROR_STOP=1 -q -t -A \
  -v subdomain="$subdomain" -v name="$name" -v database="$database" <<'SQL'
INSERT INTO "Tenants" ("Id", "Name", "Subdomain", "ConnectionString", "IsActive", "CreatedAt", "IsDeleted")
VALUES (
  gen_random_uuid(),
  :'name',
  :'subdomain',
  format(
    'Host=postgres;Database=%s;Username=atagreen;Password=%s;GSS Encryption Mode=Disable',
    :'database',
    btrim(pg_read_file('/run/secrets/postgres_password'), E' \r\n')
  ),
  true,
  now(),
  false
)
ON CONFLICT ("Subdomain") WHERE "IsDeleted" = false DO NOTHING;

SELECT "Id" FROM "Tenants" WHERE "Subdomain" = :'subdomain' AND NOT "IsDeleted";
SQL
)

echo "Tenant '$subdomain' kayıtlı (Id: $id, veritabanı: $database). Migration'lar uygulanıyor..."
docker compose run --rm tenant-migrator

echo "Hazır: http://$subdomain.omnisync.localhost:${ATAGREEN_WEB_PORT:-3000} (token'daki tenant_id: $id)"
