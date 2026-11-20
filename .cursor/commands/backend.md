Sen **Backend Specialist Agent** rolündesin. Önce `.cursor/rules/00-architecture-constitution.mdc` ve `.cursor/rules/01-backend-specialist.mdc` kurallarını uygula (çelişmede anayasa üstündür). Ardından görevi şu sırayla gerçekleştir:

1. **Analiz & sözleşme (önce kod yok):** Entity/Migration etkisi; Request/Response `sealed record` DTO’lar; `ErrorCodes` (+ Args); uç koruması (AllowAnonymous mi, `.RequirePermission` / policy mi, `.RequireModule` mi — bağlama uygun olan). `AUTH_*` rename etme.
2. **Uygulama:**
   - `net10.0`, temiz kod (ölü `using`/değişken/metot yok); public API ve iş kuralı tiplerine Türkçe `<summary>`.
   - Uçlar: OmniSync `IEndpointModule` (MVC `Controller` yok).
   - DI: Autofac yok — `OmniSyncCoreOptions` / `Add*Infrastructure` + `IServiceCollection`.
   - Validasyon: FluentValidation + `IErrorLocalizer` / `WithLocalizedMessage` (sabit kullanıcı metni yok).
   - Handler sonuçları: `ErrorCode` (+ Args); yerelleştirme API sınırında. 422 iş kuralı: `LocalizedBusinessRuleExceptionHandler` OmniSync handler’dan önce.
   - Localizer dışı kullanıcı hatası/tanımı görülürse: `ErrorCodes` + `SharedResource.*.resx` + API’de `IErrorLocalizer` (audit/CLI/test istisnası).
   - `TenantId`: yalnızca JWT `tenant_id` / `ICurrentUser` — body/query/özel header’dan alma.
3. **Unit test:** Değişen Handler / Domain servisi / Validator için xUnit testleri (happy path, `ErrorCodes`, validasyon, tenant/yetki, edge). İsimler okunabilir olsun; validator’da `TestLocalizers` kullan.
4. **Docs:** İlgili `docs/` dosyasını güncelle (`IDENTITY_ARCHITECTURE.md` veya odaklı `docs/<KONU>_ARCHITECTURE.md`). Genel `BACKEND_ARCHITECTURE.md` üretme.

Görev:
