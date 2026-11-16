# AtaGreen Tech 🌾🚜

**AtaGreen Tech** is an enterprise-grade, modular, multi-tenant SaaS framework and platform designed for smart agriculture, irrigation systems automation, and farm resource management. Inspired by the engineering standards of Atabey Mühendislik.

## 🏗️ Architecture & Tech Stack

- **Framework:** .NET 10+ (Modular Monolith / Clean Architecture)
- **Database:** PostgreSQL with EF Core Multi-Tenant Migrations
- **Security:** JWT Authentication, Refresh Token Rotation, Dynamic CORS, PBKDF2 Hashing
- **Package Management:** GitHub Packages (`GITHUB_USERNAME` & `GITHUB_PACKAGES_TOKEN` required)
- **API Standards:** OpenAPI / Swagger UI, Standardized Paged Result Envelopes
- **Web Client:** React 19 + Vite + TypeScript + Ant Design (`client/`)
- **Containerization:** Docker & Docker Compose ready

## 🚀 Getting Started

### Prerequisites
- .NET 10 SDK
- Docker & Docker Compose
- PostgreSQL

### Local Development Setup
1. Clone the repository:
   ```bash
   git clone [https://github.com/your-username/atagreen-tech.git](https://github.com/your-username/atagreen-tech.git)
   cd atagreen-tech
   ```

### Running with Docker Compose

The stack (`docker-compose.yml`) starts PostgreSQL, applies master and tenant migrations with one-off migrator
containers (Stock + User), then starts **stock-service** and **user-service** once the database is healthy, and finally
the web client (nginx routes `/api/v1/auth|users|…` to UserService, other `/api/` to StockService).

```bash
deploy/generate-secrets.sh   # creates secrets/ (database password, connection string, JWT signing key)
GITHUB_USERNAME=<user> GITHUB_PACKAGES_TOKEN=$(gh auth token) docker compose up -d --build
docker compose ps            # postgres, stock-service and web: healthy; migrators: Exited (0)
curl -s http://127.0.0.1:5001/health/ready
open http://atagreen.omnisync.localhost:3000   # web client for the "atagreen" tenant
```

- The image is a multi-stage build on the chiseled .NET 10 runtime: no shell or package manager, runs as the non-root
  `app` user (UID 1654) on a read-only root filesystem. The GitHub Packages token is passed to `dotnet restore` as a
  BuildKit secret and never stored in the image.
- Secrets are mounted as files under `/run/secrets` and read by OmniSync as configuration; nothing sensitive is kept in
  `docker-compose.yml` or environment variables. Optional settings (ports, image tag, proxy network) are in
  `.env.example`.
- `init: true` runs tini as PID 1 so `docker stop` delivers SIGTERM to the app, which finishes in-flight requests before
  exiting (`stop_grace_period: 35s`).
- Container health is checked with `dotnet AtaGreenTech.StockService.Api.dll healthcheck`, which probes `/health/ready`
  (master and tenant database connectivity). `/health/live` only reports that the process responds.
- PostgreSQL is reachable only on the internal `backend` network; the stock service is published on
  `127.0.0.1:5001` for a reverse proxy that terminates TLS. In Production, tenants are resolved from the subdomain
  (e.g. `atagreen.omnisync.com`) and plain HTTP requests are redirected to HTTPS.
- Tenants are the single label directly below a root domain listed in `TenantResolution:RootDomains`:
  `omnisync.com` and `omnisync.com.tr` (`appsettings.json`), plus `omnisync.localhost` locally (Development settings
  and `ATAGREEN_LOCAL_ROOT_DOMAIN` in Compose). Root domains themselves, `www`, deeper subdomains
  (`api.atagreen.omnisync.com`) and other hosts carry no tenant. The client applies the same rule in
  `client/src/core/tenant.ts`, so a new root domain must be added in both places.
- Add a tenant with `deploy/add-tenant.sh <subdomain> "<Name>" [admin_email] [admin_password]` (e.g.
  `deploy/add-tenant.sh atagreen "Atagreen"`, then open `http://atagreen.omnisync.localhost:3000`). It registers the
  tenant in the master database with its own database (`atagreen_stock_<subdomain>`), runs Stock + User tenant
  migrators, seeds Admin/Manager/Viewer + default Admin (`Auth:Bootstrap` / `AUTH_BOOTSTRAP_PASSWORD`), and verifies
  login. Re-running is idempotent. After deploying new tenant migrations only, use
  `docker compose run --rm tenant-migrator` and `docker compose run --rm user-tenant-migrator`. Manual seed:
  `docker compose run --rm --no-deps user-service seed-tenant <subdomain>`.
- The `web` service is an unprivileged nginx (UID 101, read-only root filesystem) published on `127.0.0.1:3000`
  (`ATAGREEN_WEB_PORT`). It serves the client and forwards `/api/` to the stock service, so the browser talks to a
  single origin and no CORS configuration is needed. The `Host` header is passed through, so tenants are resolved from
  the subdomain exactly as for direct API calls. Its `server_name` (`ATAGREEN_SERVER_NAMES`) covers `omnisync.com`,
  `omnisync.com.tr`, the local root domain and all their subdomains; any other host (IP address, unknown domain,
  `localhost`) gets `404`, only `/healthz` answers there for the container health check.
  Locally, `*.localhost` subdomains resolve to `127.0.0.1` in Chrome and Firefox; otherwise add an `/etc/hosts` entry.
- In production, point the server's reverse proxy for `*.omnisync.com` and `*.omnisync.com.tr` at this port:
  `deploy/nginx/omnisync.conf.example` (with `deploy/nginx/atagreen-proxy.conf`) terminates TLS with wildcard
  certificates, redirects HTTP to HTTPS and forwards `Host`, `X-Forwarded-For` and `X-Forwarded-Proto`.

## Kimlik Doğrulama ve Yetkilendirme (Identity & IAM)

Merkezi kimlik ve RBAC **UserService** (.NET 10 Minimal API) üzerindedir; StockService ve client aynı JWT sözleşmesini
tüketir. Detaylı akış ve secret tablosu: [`docs/IDENTITY_ARCHITECTURE.md`](docs/IDENTITY_ARCHITECTURE.md).

### Mimari genel bakış

- **Token’lar:** UserService kısa ömürlü JWT `accessToken` + tek kullanımlık `refreshToken` üretir
  (`iss`: `AtaGreenTech.UserService`, `aud`: `AtaGreenTech.Apps`, imza: `JwtSettings:SigningKey`).
- **Refresh rotation & reuse detection:** Her `/api/v1/auth/refresh` yeni çift üretir; kullanılmış refresh tekrar
  sunulursa ilgili `FamilyId` oturum ailesi iptal edilir.
- **İzinler:** JWT `permission` claim’leri (`users.read`, `stock.write`, `stock.*`, `*`) client’ta
  `RequireAuth` / `PermissionGuard` ve StockService policy’lerinde (`perm:stock.read|write|delete`) doğrulanır.
- **MFA:** TOTP (authenticator) + tek kullanımlık kurtarma kodları; `/account/security` üzerinden yönetilir.
- **Tenant seed:** `deploy/add-tenant.sh` veya `seed-tenant` CLI Admin/Manager/Viewer + varsayılan Admin’i idempotent oluşturur.

### Yerel geliştirmede giriş

1. Secret’ları oluşturup yığını ayağa kaldırın:

   ```bash
   deploy/generate-secrets.sh
   GITHUB_USERNAME=<user> GITHUB_PACKAGES_TOKEN=$(gh auth token) docker compose up -d --build
   ```

2. Tenant + Admin seed (örnek `atagreen`; varsayılan Admin `admin@omnisync.com` / `AUTH_BOOTSTRAP_PASSWORD` veya `Admin123!`):

   ```bash
   deploy/add-tenant.sh atagreen "Atagreen"
   # veya: deploy/add-tenant.sh acme "Acme" admin@acme.com 'StrongPass123!'
   ```

3. Tarayıcıdan tenant adresini açın ve giriş yapın:

   `http://atagreen.omnisync.localhost:3000/login`

Client `sessionStore` access/refresh token’ları `sessionStorage`’da tutar; Axios 401 interceptor’ı tek uçuşluk refresh
kuyruğu çalıştırır. Eski `VITE_DEFAULT_ACCESS_TOKEN` / manuel JWT yapıştırma akışı kaldırılmıştır — geliştirme girişi
yalnızca `/login` (ve gerekirse MFA) ile yapılır.

### Web Client Development

```bash
cd client
cp .env.example .env.local   # optional: API address and development tenant
npm install
npm run dev                  # http://localhost:5173 — /api → Stock (:5154) + User (:5155) proxy
npm run lint && npm test && npm run build
```

- `src/core` holds cross-cutting code: the Axios client (`core/api`), session/tenant/language hooks (`core/context`)
  and UI translations (`core/i18n`). `src/services` wraps API endpoints, `src/features` contains pages and
  `src/components` shared UI.
- Every request carries `Authorization: Bearer <token>` (after login) and `Accept-Language`. The tenant comes from the
  address (`window.location.hostname`, e.g. `atagreen.omnisync.com` → `atagreen`) and reaches the API through the
  `Host` header, so no tenant header is sent. Only the development server (`npm run dev`) lets you pick a tenant on an
  address without one (`localhost:5173`, defaulting to `VITE_DEV_TENANT`) and sends it as `X-Tenant`, which APIs honour
  only in Development. A built client never does: on an address without a tenant the sign-in page asks you to open
  your tenant address (e.g. `<tenant>.omnisync.localhost:3000`).
- Errors are normalised into `ApiError`: `400` keeps field errors for forms, `422` business rules are translated by
  their `resourceKey`. A `401` first tries a single-flight refresh; if refresh fails, the session is cleared and the
  user is sent to `/login`. Missing role/permission on a route yields `/unauthorized` (403).
- Required JWT claims: `sub`, `email`, `role`, `tenant_id`, plus granular `permission` values for RBAC.

