# OmniSync / AtaGreenTech Identity & Secret Yönetimi

Bu rehber UserService merkezli kimlik doğrulama, JWT sözleşmesi, client oturum yenileme ve secret enjeksiyonunu
özetler. Yerel kurulum adımları için kök [`README.md`](../README.md) içindeki **Kimlik Doğrulama ve Yetkilendirme**
bölümüne bakın.

## 1. Uçtan uca kimlik akışı

### 1.1 Login — `POST /api/v1/auth/login`

- Gövde: `email`, `password`; MFA açıksa ek olarak `totpCode` veya `recoveryCode`.
- Tenant, istek `Host` başlığından çözülür (`atagreen.omnisync.localhost` → `atagreen`). Development’ta adres
  tenant taşımıyorsa (`localhost:5173`) client `X-Tenant` gönderebilir; Production’da header yok sayılır.
- İlk başarılı login (veya `seed-tenant` / `add-tenant.sh`) tenant DB’de RBAC kataloğunu (Admin / Manager / Viewer +
  izinler) ve varsayılan Admin’i idempotent sağlar.
- Ardışık hatalı şifre: `Auth:MaxFailedAccessAttempts` (varsayılan 5) sonrası `Auth:LockoutMinutes` (15) kilit.
- MFA zorunluysa yanıt `401` + `mfaRequired` / `AUTH_MFA_REQUIRED`; ikinci istekte TOTP veya kurtarma kodu ile tamamlanır.
- Başarı: `accessToken`, `refreshToken`, `expiresAt` (camelCase JSON).

### 1.2 Token yenileme — `POST /api/v1/auth/refresh`

- Gövde: `refreshToken`.
- **Rotation:** her başarılı yenilemede yeni access + refresh üretilir; eski refresh iptal edilir (aynı `FamilyId`).
- **Reuse detection:** daha önce kullanılmış/iptal bir refresh tekrar sunulursa ailedeki tüm oturumlar iptal edilir;
  istemci yeniden login olmalıdır.
- Client: Axios 401 interceptor’ı eşzamanlı istekleri tek uçuşluk refresh kuyruğunda birleştirir; refresh de 401 olursa
  oturum temizlenir ve `/login`’e yönlendirilir.

### 1.3 Mikroservis yetkilendirme (StockService ve diğerleri)

- `Authorization: Bearer <access_token>`.
- Doğrulama: `JwtSettings:Issuer` = `AtaGreenTech.UserService`, `Audience` = `AtaGreenTech.Apps`, ortak
  `JwtSettings:SigningKey` (HS256; Compose’ta `secrets/jwt_signing_key` → `JwtSettings__SigningKey`).
- Claim’ler: `sub`, `email`, `role` (çoklu), `tenant_id`, `permission` (çoklu; örn. `stock.read`, `users.write`).
- Policy eşlemesi: `perm:stock.read|write|delete` ve security-test alias’ları (`StockReadPolicy` vb.); joker
  `stock.*` / `*` `PermissionMatcher` ile desteklenir.
- Smoke: `GET /api/v1/security-test/protected|write-check|delete-check`.

### 1.4 Oturum ve çıkış

| Uç | Açıklama |
| --- | --- |
| `GET /api/v1/sessions` | Aktif oturumlar (mevcut cihaz `X-Refresh-Token` ile işaretlenebilir) |
| `POST /api/v1/sessions/{id}/revoke` | Tekil oturum iptali |
| `POST /api/v1/sessions/revoke-others` | Diğer cihazları kapat |
| `POST /api/v1/auth/logout` | Sunulan refresh’i iptal eder |
| `POST /api/v1/auth/change-password` | Şifre değişimi (diğer oturumları düşürebilir) |
| MFA setup / verify / disable | Authenticator + kurtarma kodları |

Client’ta **Oturumlarım & Güvenlik** (`/account/security`) bu uçları kullanır.

### 1.5 Frontend koruma

- `RequireAuth`: oturum yok → `/login`; rol/izin yetmez → `/unauthorized` (403).
- `PermissionGuard`: sayfa içi buton/bölüm gizleme (`stock.write` vb.).
- Menü / komut paleti: `navigation` config + JWT `permission` / `role` filtresi.

## 2. Secret ve ortam değişkenleri

Production / staging’de Development imza anahtarı veya boş provisioning anahtarı kullanılmamalıdır.
Compose gizli değerleri `deploy/generate-secrets.sh` ile `secrets/` altına yazar ve container’a file secret olarak
bağlar; isteğe bağlı port/tag ayarları kök `.env` dosyasındadır (`cp .env.example .env`).

| Anahtar / dosya | Servis | Açıklama |
| --- | --- | --- |
| `secrets/jwt_signing_key` → `JwtSettings__SigningKey` | UserService, StockService | HS256 imza; **her iki serviste aynı** olmalı. En az ~32 byte rastgele. |
| `JwtSettings__Issuer` / `Audience` | User + Stock | Sabit sözleşme: `AtaGreenTech.UserService` / `AtaGreenTech.Apps` (`JwtContract`). |
| `secrets/master_connection_string` → `ConnectionStrings__MasterDb` | User + Stock + migrator’lar | Master (tenant kataloğu) PostgreSQL. Tenant DB’ler master’daki connection string ile açılır. |
| `secrets/postgres_password` | postgres | DB kullanıcı parolası; master/tenant connection string’lerle uyumlu tutulur. |
| `Auth__Bootstrap__Password` / `AUTH_BOOTSTRAP_PASSWORD` | UserService | Boş tenant’ta / `seed-tenant` varsayılan Admin parolası (`PasswordRules`: min 8, büyük/küçük, rakam, özel). |
| `InternalProvisioning__ApiKey` / `INTERNAL_PROVISION_API_KEY` | UserService | `POST /api/v1/internal/tenants/seed` + `X-Internal-Provision-Key`. Boşsa endpoint 401. |
| `FeatureFlags__EnableOidcSso` | UserService | V1’de `false`; OIDC uçları 501. |
| `VITE_API_BASE_URL` | Client | Varsayılan `/api/v1` (aynı origin; Vite/nginx proxy). |
| `VITE_DEV_TENANT` | Client (yalnızca `npm run dev`) | Host’ta tenant yokken `X-Tenant`. |
| `DEV_API_PROXY_TARGET` / `DEV_USER_API_PROXY_TARGET` | Client Vite | Stock / User yerel API adresleri. |

> Not: Konfigürasyon anahtarı `JwtSettings:SigningKey`’dir (`SecretKey` değil). Master bağlantı
> `ConnectionStrings:MasterDb`’dir (`DefaultConnection` değil).

## 3. Operasyonel komutlar

```bash
# Secret dosyaları (idempotent: var olanlara dokunmaz)
deploy/generate-secrets.sh

# Tenant kaydı + Stock/User migration + RBAC/Admin seed + login smoke
deploy/add-tenant.sh atagreen "Atagreen"
deploy/add-tenant.sh acme "Acme" admin@acme.com 'StrongPass123!'

# Yalnızca identity seed (tenant + migration’lar zaten var)
docker compose run --rm --no-deps user-service seed-tenant atagreen
docker compose run --rm --no-deps user-service seed-tenant acme --email=admin@acme.com --password='StrongPass123!'
```

E2E kontrat testleri: `tests/AtaGreenTech.IntegrationTests` (Testcontainers; Docker gerekir).

## 4. Dokümantasyon temizliği (checklist)

- [x] `VITE_DEFAULT_ACCESS_TOKEN` / manuel JWT yapıştırma client env ve README akışından çıkarıldı; giriş `/login` + session/refresh.
- [x] Swagger/curl ile token kopyalama birincil local login yolu değil; `add-tenant.sh` + tarayıcı `/login` belgelendi.
- [x] Ortak JWT sözleşmesi (`Issuer` / `Audience` / `SigningKey` / `permission` claim) ve secret tablosu bu dosyada.
