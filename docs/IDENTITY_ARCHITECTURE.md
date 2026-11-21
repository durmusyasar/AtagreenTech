# OmniSync / AtaGreenTech Identity & Secret Yönetimi

Bu rehber UserService merkezli kimlik doğrulama, JWT sözleşmesi, client oturum yenileme ve secret enjeksiyonunu
özetler. Yerel kurulum adımları için kök [`README.md`](../README.md) içindeki **Kimlik Doğrulama ve Yetkilendirme**
bölümüne bakın.

## 1. Uçtan uca kimlik akışı

### 1.1 Login — `POST /api/v1/auth/login`

- HTTP gövde DTO’ları: `Api/Contracts/Auth/*Request.cs` (endpoint dosyasına gömülmez).
- Gövde: `email`, `password`; MFA açıksa ek olarak `totpCode` veya `recoveryCode`.
- Tenant, istek `Host` başlığından çözülür (`atagreen.omnisync.localhost` → `atagreen`). Development’ta adres
  tenant taşımıyorsa (`localhost:5173`) client `X-Tenant` gönderebilir; Production’da header yok sayılır.
- İlk başarılı login (veya `seed-tenant` / `add-tenant.sh`) tenant DB’de RBAC kataloğunu ve varsayılan Admin’i
  idempotent sağlar. Ortak yol: `TenantIdentitySeeder.EnsureAsync` → `RbacCatalog.EnsureAsync`
  (`Permissions.Catalog` + `DefaultRoleGrants` / `DefaultRoleDescriptions`) → ardından Admin upsert/bootstrap
  (`TenantIdentitySeedMode` `Features/Auth/TenantIdentitySeed.cs`: login’de `BootstrapIfEmpty`,
  provision/`seed-tenant` CLI’de `UpsertAdmin`). CLI sonucu OmniSync `RunInTenantScopeAsync<T>` ile
  döner. `RbacCatalog` ikinci çağrıda izin/rol/`RolePermission` satırlarını çoğaltmaz; eksik grant’ları tamamlar.
- Ardışık hatalı şifre: `Auth:MaxFailedAccessAttempts` (varsayılan 5) sonrası `Auth:LockoutMinutes` (15) kilit.
- MFA zorunluysa yanıt `401` + `mfaRequired` / `AUTH_MFA_REQUIRED`; ikinci istekte TOTP veya kurtarma kodu ile tamamlanır.
- Hata gövdesi: `AuthAttemptResult` → `code` (`AUTH_*` / `SESSION_*`) + `ErrorArgs`; `message` yalnızca API
  sınırında `IErrorLocalizer` / `SharedResource` ile üretilir (sonuçta kullanıcı metni alanı yok, #71).
  Tenant host çözülemezse `AUTH_TENANT_UNRESOLVED` (400). Audit `details` → `ErrorCodes.Audit.*` + `IErrorLocalizer` (#71).
- İstek validasyonu (`LoginCommandValidator`): `FIELD_EMAIL` / `FIELD_PASSWORD` + `VALIDATION_REQUIRED` /
  `VALIDATION_INVALID` / `VALIDATION_MAX_LENGTH` (`IErrorLocalizer`).
- Başarı: `accessToken`, `refreshToken`, `expiresAt` (camelCase JSON).

### 1.2 Token yenileme — `POST /api/v1/auth/refresh`

- Gövde: `refreshToken`. Validasyon: `FIELD_REFRESH_TOKEN` + `VALIDATION_REQUIRED` / `VALIDATION_MAX_LENGTH`.
- **Rotation:** her başarılı yenilemede yeni access + refresh üretilir; eski refresh iptal edilir (aynı `FamilyId` =
  access `sid`). Üretim: `RefreshTokenFactory` (32 bayt entropi Base64 plaintext; DB’de SHA-256 hex hash).
- **Reuse detection:** daha önce kullanılmış/iptal bir refresh tekrar sunulursa ailedeki tüm oturumlar iptal edilir
  (DB `Revoke` + access denylist `sid`); yanıt `SESSION_REUSE_DETECTED`. Ömür: `Auth:RefreshTokenDays` (> 0).
- Bilinmeyen/süresi dolmuş refresh → `SESSION_EXPIRED`; pasif kullanıcı → `AUTH_ACCOUNT_INACTIVE` (+ aile/denylist iptali).
  Tenant yoksa `AUTH_TENANT_UNRESOLVED` (400). Mesajlar API’de `IErrorLocalizer` (#71).
- Client: Axios 401 interceptor’ı eşzamanlı istekleri tek uçuşluk refresh kuyruğunda birleştirir; refresh de 401 olursa
  oturum temizlenir ve `/login`’e yönlendirilir.

### 1.3 Mikroservis yetkilendirme (StockService ve diğerleri)

- `Authorization: Bearer <access_token>`.
- Doğrulama: `JwtSettings:Issuer` = `AtaGreenTech.UserService`, `Audience` = `AtaGreenTech.Apps`, ortak
  `JwtSettings:SigningKey` (HS256; Compose’ta `secrets/jwt_signing_key` → `JwtSettings__SigningKey`).
- Claim’ler: `sub`, `email`, `role` (çoklu), `tenant_id`, `permission` (çoklu; örn. `stock.read`, `users.write`),
  `#70` oturum: `sid` (session/family), `sst` (security stamp).
- Mint: `JwtAccessTokenIssuer` (`JwtContract` + OmniSync `JwtTokenSigning`); rol/izin claim’leri
  case-insensitive distinct; ömür `Auth:AccessTokenMinutes` (≤0 → `JwtContract.DefaultAccessTokenMinutes`).
- `permission` claim değerleri `UserPermissionResolver` ile üretilir: kullanıcının aktif rollerindeki
  `RolePermission` birleşimi (soft-delete satırlar hariç), case-insensitive distinct + sıralı.
- Policy eşlemesi: `perm:stock.read|write|delete` ve security-test alias’ları (`StockReadPolicy` vb.); joker
  `stock.*` / `*` `PermissionMatcher` ile desteklenir.
- Smoke: `GET /api/v1/security-test/protected|write-check|delete-check`.

### 1.3b Ben — `GET /api/v1/auth/me`

- RequireAuthorization. Profil tenant DB’den okunur; `TenantId` yalnızca host/`ICurrentTenant` (#71).
- Hatalar `GetMeResult.ErrorCode` ile API’de lokalize edilir: oturum yok / kullanıcı yok →
  `AUTH_INVALID_CREDENTIALS` (401); tenant yok → `AUTH_TENANT_UNRESOLVED` (400); pasif →
  `AUTH_ACCOUNT_INACTIVE` (403).
- Rol + izin tek Include grafiğinden (`UserPermissionResolver.FromLoadedUser`).

### 1.3b2 Public linkler (`AuthPublicLinks`)

- E-posta linkleri: `Auth:PublicAppBaseUrl` + isteğe bağlı `{tenantIdentifier}.{host}` + path + `?token=`.
- `localhost` / tek etiketli host’ta subdomain yok; zaten tenant önekli host tekrarlanmaz.
- Boş path/token/base → `VALIDATION_REQUIRED`; geçersiz base URL → `VALIDATION_INVALID` (#71).

### 1.3c E-posta doğrulama — `POST /api/v1/auth/verify-email`


- AllowAnonymous. Gövde `Token`; FV: `FIELD_TOKEN` / `VALIDATION_REQUIRED` / `VALIDATION_MAX_LENGTH`.
- Tenant host’tan; yoksa `AUTH_TENANT_UNRESOLVED`. Geçersiz/süresi dolmuş jeton → `AUTH_VERIFY_TOKEN_INVALID`.
- Başarı: `MessageResult.OkCode(AUTH_VERIFY_EMAIL_SUCCESS)` → API lokalize (#71). Challenge temizlenir.

### 1.3d Doğrulamayı yeniden gönder — `POST /api/v1/auth/resend-verification`

- AllowAnonymous + rate limit. Gövde `Email`; FV: `FIELD_EMAIL` / `VALIDATION_*` (Login ile aynı).
- Tenant yok → `AUTH_TENANT_UNRESOLVED`. Kullanıcı yok / pasif / zaten doğrulanmış → yine
  `AUTH_RESEND_VERIFICATION_SUCCESS` (enumeration önleme; mail gitmez).
- Uygun hesapta yeni challenge + mail; başarı kodu aynı → API lokalize (#71).

### 1.3e Şifremi unuttum — `POST /api/v1/auth/forgot-password`

- AllowAnonymous + rate limit. Gövde `Email`; FV: `FIELD_EMAIL` / `VALIDATION_*`.
- Tenant yok → `AUTH_TENANT_UNRESOLVED`. Kullanıcı yok / pasif → yine
  `AUTH_FORGOT_PASSWORD_SUCCESS` (enumeration önleme; `AUTH_EMAIL_NOT_FOUND` dönülmez; mail gitmez).
- Uygun hesapta reset challenge + mail; başarı kodu aynı → API lokalize (#71).

### 1.3f Davet kabul — `POST /api/v1/auth/accept-invite`

- AllowAnonymous. Gövde `Token` + `Password` + `FirstName`/`LastName`; FV: `FIELD_TOKEN` /
  `ApplyStrongPassword` / `FIELD_FIRST_NAME` / `FIELD_LAST_NAME` + `VALIDATION_*`.
- Tenant yok → `AUTH_TENANT_UNRESOLVED`. Geçersiz/süresi dolmuş davet → `AUTH_INVITE_TOKEN_INVALID`.
- Başarı: `AcceptInvitation` (parola, profil, e-posta doğrulanmış, aktif);
  `MessageResult.OkCode(AUTH_ACCEPT_INVITE_SUCCESS)` → API lokalize (#71).

### 1.3g Şifre sıfırla — `POST /api/v1/auth/reset-password`

- AllowAnonymous + rate limit. Gövde `Token` + `NewPassword`; FV: `FIELD_TOKEN` / `VALIDATION_*` +
  `PasswordRules` / `ApplyStrongPassword`.
- Tenant yok → `AUTH_TENANT_UNRESOLVED`. Geçersiz/süresi dolmuş jeton → `AUTH_RESET_TOKEN_INVALID`.
- Başarı: parola değişir, tüm refresh + access `sid` iptal, stamp yayın;
  `MessageResult.OkCode(AUTH_RESET_PASSWORD_SUCCESS)` → API lokalize (#71).
- Challenge jetonları: `EmailVerificationTokenFactory` (verify/reset/invite; boş plaintext →
  `VALIDATION_REQUIRED`).




### 1.4 Oturum ve çıkış

| Uç | Açıklama |
| --- | --- |
| `GET /api/v1/sessions` | Aktif oturumlar (mevcut cihaz `X-Refresh-Token` ile işaretlenebilir) |
| `POST /api/v1/sessions/{id}/revoke` | Tekil oturum iptali |
| `POST /api/v1/sessions/revoke-others` | Diğer cihazları kapat |
| `POST /api/v1/auth/logout` | Refresh ailesi + access `sid` denylist iptali (AllowAnonymous, idempotent). Tenant yok → `AUTH_TENANT_UNRESOLVED`. Gövde FV: `FIELD_REFRESH_TOKEN` / `VALIDATION_*`. Başarı: `MessageResult.OkCode(AUTH_LOGOUT_SUCCESS)` → API lokalize (#71). |
| `POST /api/v1/auth/change-password` | RequireAuthorization. FV: `FIELD_CURRENT_PASSWORD` / `ApplyStrongPassword` / `AUTH_PASSWORD_SAME_AS_CURRENT`. Yanlış mevcut → `AUTH_PASSWORD_MISMATCH` (400, 401 değil). Başarı: `AUTH_CHANGE_PASSWORD_SUCCESS` + tüm oturum iptali (`SessionRevocation` + stamp) (#71). |
| `POST /api/v1/auth/mfa/setup` | RequireAuthorization. Secret + QR URI + kurtarma kodları. MFA açık → `AUTH_MFA_ALREADY_ENABLED`; oturum yok → `AUTH_INVALID_CREDENTIALS`. `MfaSetupResult.ErrorCode` → API lokalize (#71). |
| `POST /api/v1/auth/mfa/verify` | RequireAuthorization. Enrollment TOTP → MFA aç. FV: `FIELD_CODE` / `VALIDATION_*`. Zaten açık → `AUTH_MFA_ALREADY_ENABLED`; kurulum yok → `AUTH_MFA_SETUP_REQUIRED`; kod hatalı → `AUTH_MFA_INVALID`; oturum yok → `AUTH_INVALID_CREDENTIALS`. Başarı: `AUTH_VERIFY_MFA_SUCCESS` (#71). |
| `POST /api/v1/auth/mfa/disable` | RequireAuthorization. FV: `FIELD_PASSWORD` / `VALIDATION_*`. Yanlış parola → `AUTH_PASSWORD_MISMATCH` (400, 401 değil); zaten kapalı → `AUTH_MFA_ALREADY_DISABLED`; oturum yok → `AUTH_INVALID_CREDENTIALS`. Başarı: `AUTH_DISABLE_MFA_SUCCESS`; audit `AUDIT_MFA_DISABLED` (#71). |
| `GET/POST …/auth/sso/{provider}` (+ callback) | AllowAnonymous iskelet. `OidcPlaceholderService` → `NotImplementedException(SSO_NOT_IMPLEMENTED)`; API yalnızca bu kodu 501 + lokalize eder (`when` filtresi). `MessageResult` uçları `ToMessageResult` / `BadRequestError` (#71). |

Client’ta **Oturumlarım & Güvenlik** (`/account/security`) bu uçları kullanır.

### 1.5 Access lifecycle (#70 — `AccessTokenLifecycle`)

- **Stamp yayın:** login / refresh / şifre değişimi / `AssignUserRoles` / `UpdateUser` (rol değişince) / rol-izin güncellemede `PublishStampAsync` → master `UserAccessStamps`
  (`sst` doğrulaması; Stock tenant User tablosuna bakmaz). Tenant yoksa `AUTH_TENANT_UNRESOLVED`.
  Boş `userId` / `securityStamp` → `VALIDATION_REQUIRED` (`FIELD_USER_ID` / `FIELD_SECURITY_STAMP`).
- **Oturum denylist:** `RevokeSessionAsync` / `RevokeFamiliesAsync` / `RevokeAllSessionsAsync` → `sid` iptali;
  TTL = access token ömrü (`Auth:AccessTokenMinutes`). Logout, session revoke ve “diğer oturumları kapat” bu yolu kullanır.
  Boş `sessionId` / `userId` → `VALIDATION_REQUIRED` (`FIELD_SESSION_ID` / `FIELD_USER_ID`).
- Refresh satırları tenant DB’de `Revoke()` edilir; access tarafı denylist ile düşer.

### 1.5b Domain invariantleri (#71)

- Domain sabit İngilizce kullanıcı metni fırlatmaz; `DomainErrorCodes` taşır (değerler SharedKernel
  `ErrorCodes` / `SharedResource` ile birebir). Domain SharedKernel’e referans vermez.
- `User`: MFA zaten açık → `AUTH_MFA_ALREADY_ENABLED`; kurulum yok → `AUTH_MFA_SETUP_REQUIRED`;
  davet yok → `AUTH_INVITE_TOKEN_INVALID`; zorunlu/max alan → `VALIDATION_REQUIRED` / `VALIDATION_MAX_LENGTH`;
  geçmiş expiry → `VALIDATION_INVALID`.
- `UserRole` / `RolePermission`: boş kimlikler → `VALIDATION_REQUIRED`.
- `RefreshToken`: boş `userId` / `familyId` / `tokenHash` → `VALIDATION_REQUIRED`; geçmiş
  `expiresAt` → `VALIDATION_INVALID`; hash max → `VALIDATION_MAX_LENGTH`.
- `Role` / `Permission`: boş ad → `VALIDATION_REQUIRED`; ad/açıklama max → `VALIDATION_MAX_LENGTH`.
- `AuditLog`: boş `action` / `email` → `VALIDATION_REQUIRED`; action/email max →
  `VALIDATION_MAX_LENGTH`; uzun `ipAddress` / `details` truncate edilir (teşhis alanı).
- Master katalog şekilleri (`Module`, `MenuDefinition`, `MenuLanguage`, `Language`,
  `ModuleLanguage`): EF aggregate değildir; SQL + uzunluk sabitleri / `nameof` için tutulur.
- Master `TenantModuleSubscriptions` (#66/#67) yalnızca SQL (`MasterNavigationSchema`,
  `NavigationMenuCatalog`, Stock `ModuleSubscriptionChecker`); Domain’de EF entity yok.
  Liste/çekmece sözleşmesi ve efektif aktiflik: `docs/TENANT_MODULE_SUBSCRIPTIONS.md`
  (`activeModuleCount` / `activeModuleBadges` / `isEffectivelyActive`).

### 1.6 Parola özeti (`IPasswordHasher` / `Pbkdf2PasswordHasher`)

- Algoritma: PBKDF2-HMAC-SHA256, **100.000** iterasyon, 16 bayt salt, 32 bayt özet.
- Saklama: yeni hash’ler `v1.{saltBase64}.{hashBase64}`; doğrulama eski `{salt}.{hash}` biçimini de kabul eder
  (iterasyon yükseltmesi için versiyon alanı hazır).
- Karşılaştırma: `CryptographicOperations.FixedTimeEquals` (zamanlama sızıntısına karşı).
- Politika (uzunluk/karakter): `PasswordRules` / FE `passwordPolicy.ts` — hasher yalnızca özetler.

### 1.7 Kimlik audit (`IIdentityAuditLogger`)

- Login başarı/başarısızlık, MFA, şifre sıfırlama, davet, rol/izin değişimi ve tenant identity seed
  olayları tenant DB `AuditLog` tablosuna append edilir (`AuditLog.Actions.*`).
- `IdentityAuditLogger` yalnızca `AddAsync` yapar; **ayrı `SaveChanges` çağırmaz**. Kayıt, komut
  handler / seeder UoW commit’i ile aynı transaction’da kalıcı olur (ör. başarısız login + lockout).
- `details` alanı `ErrorCodes.Audit.*` + `IErrorLocalizer` ile yazılır (sabit metin yok; Args’lı `Localize` çağrılarında `fallback: null`).
  İstek kültüründe kalıcılaşır; `action` alanı makine kodu (`AuditLog.Actions.*`) olarak kalır.

### 1.8 Frontend koruma

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
| `Auth__Bootstrap__Password` / `AUTH_BOOTSTRAP_PASSWORD` | UserService | Boş tenant’ta / `seed-tenant` varsayılan Admin parolası. Politika: `PasswordRules` / FE `passwordPolicy.ts` (min 8, max 128, ASCII büyük/küçük, rakam, özel); FV yolu `ApplyStrongPassword` → `VALIDATION_PASSWORD_*`. |
| `InternalProvisioning__ApiKey` / `INTERNAL_PROVISION_API_KEY` | UserService | `POST /api/v1/internal/tenants/seed` + `X-Internal-Provision-Key`. Boşsa endpoint 401. |
| `FeatureFlags__EnableOidcSso` | UserService | V2 için ayrıldı; V1 placeholder flag’e bakmaz, her zaman `SSO_NOT_IMPLEMENTED` → 501 (#71). |
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
