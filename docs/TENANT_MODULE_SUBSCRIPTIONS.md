# Tenant modül abonelikleri (#66 / #67)

Master DB `TenantModuleSubscriptions` — platform tenant listesi ve çekmece ile **aynı efektif aktiflik** kuralı.

## Efektif aktiflik

`TenantModuleSubscriptionRules.IsEffectivelyActive`:

| Durum | Aktif mi? |
|---|---|
| `IsCore = true` | Evet (switch / expiry yok sayılır) |
| Non-core, `IsEnabled = false` | Hayır |
| Non-core, enabled, `ExpiresAtUtc` null | Evet |
| Non-core, enabled, `ExpiresAtUtc > UtcNow` | Evet |
| Non-core, enabled, `ExpiresAtUtc <= UtcNow` | Hayır (süresi dolmuş) |

Nav menü filtresi ve Stock `ModuleSubscriptionChecker` aynı semantiği kullanır.

## `GET /tenants` liste özeti

`enabledModuleCount` **kaldırıldı**. Yerine:

| Alan | Anlam |
|---|---|
| `activeModuleCount` | Core + fiilen aktif opsiyonel |
| `optionalActiveModuleCount` | Yalnızca fiilen aktif non-core |
| `expiredModuleCount` | Enabled ama `ExpiresAtUtc <= now` non-core |
| `activeModuleBadges[]` | Non-core enabled satırlar: `{ code, isExpired }` |

Yetki: `system.tenants.manage` (değişmedi).

## `GET /tenants/{id}/modules`

Her satırda ham `isEnabled` / `expiresAtUtc` yanında `isEffectivelyActive` — liste sayaçları ile tutarlı runtime bayrağı.

## FE

- Tenant tablosu: `ConnectedListPage` (`docs/LIST_PAGE_ARCHITECTURE.md`) + rozetler.
- Liste sıralama query: `sortBy` = `name` | `slug` | `isActive` | `createdAtUtc`, `sortDir` = `asc` | `desc`.
- Çekmece: süresi dolmuş abonelikte uyarı etiketi; switch hâlâ ham `isEnabled`.
