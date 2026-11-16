#!/usr/bin/env bash
# Docker Compose yığınına bir tenant ekler: master kaydı, Stock + User tenant migration'ları,
# ardından RBAC (Admin/Manager/Viewer) + varsayılan Admin seed ve isteğe bağlı login doğrulaması.
#
#   deploy/add-tenant.sh <subdomain> "<Ad>" [admin_email] [admin_password]
#     ör. deploy/add-tenant.sh atagreen "Atagreen"
#     ör. deploy/add-tenant.sh acme "Acme Corp" admin@acme.com 'StrongPass123!'
#
# Admin bilgisi verilmezse Auth:Bootstrap (AUTH_BOOTSTRAP_PASSWORD / admin@omnisync.com) kullanılır.
# Tenant <subdomain>.omnisync.com|.omnisync.com.tr|.omnisync.localhost adresinden çözülür.

set -euo pipefail

cd "$(dirname "$0")/.."

if [[ $# -lt 2 ]]; then
  echo "Kullanım: $0 <subdomain> \"<Ad>\" [admin_email] [admin_password]" >&2
  exit 64
fi

subdomain="$1"
name="$2"
admin_email="${3:-}"
admin_password="${4:-}"

USER_SERVICE_HTTP_PORT="${USER_SERVICE_HTTP_PORT:-5002}"
USER_SERVICE_URL="${USER_SERVICE_URL:-http://127.0.0.1:${USER_SERVICE_HTTP_PORT}}"
INTERNAL_PROVISION_API_KEY="${INTERNAL_PROVISION_API_KEY:-}"
ATAGREEN_LOCAL_ROOT_DOMAIN="${ATAGREEN_LOCAL_ROOT_DOMAIN:-omnisync.localhost}"
ATAGREEN_WEB_PORT="${ATAGREEN_WEB_PORT:-3000}"

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

echo "==> [1/4] Tenant '${subdomain}' master kaydı..."

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

INSERT INTO "TenantBrandings" ("TenantId", "DisplayName", "LogoUrl", "FaviconUrl", "PrimaryColor", "UpdatedAt")
SELECT t."Id", t."Name", '/favicon.svg', '/favicon.svg', '#2e7d32', now()
FROM "Tenants" t
WHERE t."Subdomain" = :'subdomain' AND NOT t."IsDeleted"
ON CONFLICT ("TenantId") DO NOTHING;

SELECT "Id" FROM "Tenants" WHERE "Subdomain" = :'subdomain' AND NOT "IsDeleted";
SQL
)

echo "    Kayıtlı (Id: $id, veritabanı: $database)."

echo "==> [2/4] Tenant migration'ları (Stock + User)..."
docker compose run --rm tenant-migrator
docker compose run --rm user-tenant-migrator

echo "==> [3/4] RBAC + varsayılan Admin seed..."

seed_args=(seed-tenant "$subdomain")
if [[ -n "$admin_email" ]]; then
  seed_args+=(--email="$admin_email")
fi
if [[ -n "$admin_password" ]]; then
  seed_args+=(--password="$admin_password")
fi

# Öncelik: çalışan UserService internal HTTP (API key varsa); aksi halde one-shot CLI.
seeded=0
if [[ -n "$INTERNAL_PROVISION_API_KEY" ]]; then
  payload=$(ADMIN_EMAIL="$admin_email" ADMIN_PASSWORD="$admin_password" TENANT="$subdomain" python3 - <<'PY'
import json, os
body = {"tenantId": os.environ["TENANT"]}
if os.environ.get("ADMIN_EMAIL"):
    body["adminEmail"] = os.environ["ADMIN_EMAIL"]
if os.environ.get("ADMIN_PASSWORD"):
    body["adminPassword"] = os.environ["ADMIN_PASSWORD"]
print(json.dumps(body))
PY
)
  http_status=$(curl -sS -o /tmp/atagreen_tenant_seed.json -w "%{http_code}" \
    -X POST "${USER_SERVICE_URL}/api/v1/internal/tenants/seed" \
    -H "Content-Type: application/json" \
    -H "Host: ${subdomain}.${ATAGREEN_LOCAL_ROOT_DOMAIN}" \
    -H "X-Internal-Provision-Key: ${INTERNAL_PROVISION_API_KEY}" \
    -d "$payload" || echo "000")

  if [[ "$http_status" == "200" ]]; then
    echo "    API seed OK: $(cat /tmp/atagreen_tenant_seed.json)"
    seeded=1
  elif [[ "$http_status" == "400" ]]; then
    echo "Hata: seed geçersiz girdi (HTTP 400): $(cat /tmp/atagreen_tenant_seed.json)" >&2
    exit 1
  else
    echo "    API seed atlandı (HTTP ${http_status}); CLI fallback..."
  fi
fi

if [[ "$seeded" -eq 0 ]]; then
  docker compose run --rm --no-deps user-service "${seed_args[@]}"
fi

echo "==> [4/4] Login doğrulaması..."
login_email="${admin_email:-admin@omnisync.com}"
login_password="${admin_password:-${AUTH_BOOTSTRAP_PASSWORD:-Admin123!}}"

login_status=$(curl -sS -o /tmp/atagreen_tenant_login.json -w "%{http_code}" \
  -X POST "${USER_SERVICE_URL}/api/v1/auth/login" \
  -H "Content-Type: application/json" \
  -H "Host: ${subdomain}.${ATAGREEN_LOCAL_ROOT_DOMAIN}" \
  -d "{\"email\":\"${login_email}\",\"password\":\"${login_password}\"}" || echo "000")

if [[ "$login_status" == "200" ]]; then
  echo "    Login OK: ${login_email} @ ${subdomain}"
else
  echo "Uyarı: Login HTTP ${login_status}. Detay: $(cat /tmp/atagreen_tenant_login.json 2>/dev/null || true)" >&2
fi

rm -f /tmp/atagreen_tenant_seed.json /tmp/atagreen_tenant_login.json

echo "Hazır: http://${subdomain}.${ATAGREEN_LOCAL_ROOT_DOMAIN}:${ATAGREEN_WEB_PORT} (tenant_id: $id)"
