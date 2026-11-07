# AtaGreen Tech 🌾🚜

**AtaGreen Tech** is an enterprise-grade, modular, multi-tenant SaaS framework and platform designed for smart agriculture, irrigation systems automation, and farm resource management. Inspired by the engineering standards of Atabey Mühendislik.

## 🏗️ Architecture & Tech Stack

- **Framework:** .NET 10+ (Modular Monolith / Clean Architecture)
- **Database:** PostgreSQL with EF Core Multi-Tenant Migrations
- **Security:** JWT Authentication, Refresh Token Rotation, Dynamic CORS, PBKDF2 Hashing
- **Package Management:** GitHub Packages (`GITHUB_USERNAME` & `GITHUB_PACKAGES_TOKEN` required)
- **API Standards:** OpenAPI / Swagger UI, Standardized Paged Result Envelopes
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
containers, then starts the stock service once the database is healthy and all migrations have succeeded.

```bash
deploy/generate-secrets.sh   # creates secrets/ (database password, connection string, JWT signing key)
GITHUB_USERNAME=<user> GITHUB_PACKAGES_TOKEN=$(gh auth token) docker compose up -d --build
docker compose ps            # postgres and stock-service: healthy; migrators: Exited (0)
curl -s http://127.0.0.1:5001/health/ready
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
  (e.g. `acme.atagreen.tech`) and plain HTTP requests are redirected to HTTPS.
- Tenant connection strings stored in the master database must use `Host=postgres` and should include
  `GSS Encryption Mode=Disable`. After adding a tenant, apply its migrations with
  `docker compose run --rm tenant-migrator`.
