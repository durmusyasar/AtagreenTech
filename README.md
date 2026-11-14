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
containers, then starts the stock service once the database is healthy and all migrations have succeeded, and finally
the web client.

```bash
deploy/generate-secrets.sh   # creates secrets/ (database password, connection string, JWT signing key)
GITHUB_USERNAME=<user> GITHUB_PACKAGES_TOKEN=$(gh auth token) docker compose up -d --build
docker compose ps            # postgres, stock-service and web: healthy; migrators: Exited (0)
curl -s http://127.0.0.1:5001/health/ready
open http://acme.omnisync.localhost:3000   # web client for the "acme" tenant
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
  (e.g. `acme.omnisync.com`) and plain HTTP requests are redirected to HTTPS.
- Tenants are the single label directly below a root domain listed in `TenantResolution:RootDomains`:
  `omnisync.com` and `omnisync.com.tr` (`appsettings.json`), plus `omnisync.localhost` locally (Development settings
  and `ATAGREEN_LOCAL_ROOT_DOMAIN` in Compose). Root domains themselves, `www`, deeper subdomains
  (`api.acme.omnisync.com`) and other hosts carry no tenant. The client applies the same rule in
  `client/src/core/tenant.ts`, so a new root domain must be added in both places.
- Tenant connection strings stored in the master database must use `Host=postgres` and should include
  `GSS Encryption Mode=Disable`. After adding a tenant, apply its migrations with
  `docker compose run --rm tenant-migrator`.
- The `web` service is an unprivileged nginx (UID 101, read-only root filesystem) published on `127.0.0.1:3000`
  (`ATAGREEN_WEB_PORT`). It serves the client and forwards `/api/` to the stock service, so the browser talks to a
  single origin and no CORS configuration is needed. The `Host` header is passed through, so tenants are resolved from
  the subdomain exactly as for direct API calls. Point the reverse proxy for `*.omnisync.com` and `*.omnisync.com.tr` at
  this port.
  Locally, `*.localhost` subdomains resolve to `127.0.0.1` in Chrome and Firefox; otherwise add an `/etc/hosts` entry.

### Web Client Development

```bash
cd client
cp .env.example .env.local   # optional: API address and development tenant
npm install
npm run dev                  # http://localhost:5173, /api is proxied to the stock service on :5154
npm run lint && npm test && npm run build
```

- `src/core` holds cross-cutting code: the Axios client (`core/api`), session/tenant/language hooks (`core/context`)
  and UI translations (`core/i18n`). `src/services` wraps API endpoints, `src/features` contains pages and
  `src/components` shared UI.
- Every request carries `Authorization: Bearer <token>` and `Accept-Language`. On `localhost` there is no subdomain,
  so the tenant is sent as `X-Tenant` (the stock service honours it only in Development); on a tenant subdomain the
  header is omitted.
- Errors are normalised into `ApiError`: `400` keeps field errors for forms, `422` business rules are translated by
  their `resourceKey`, and `401` signs the user out and redirects to the sign-in page.
- Until an identity service exists, sign in by pasting an access token issued for the stock service. The token is
  kept in `sessionStorage` and cleared when the tab closes.
