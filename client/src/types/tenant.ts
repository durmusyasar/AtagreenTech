/**
 * Aktif tenant'ın marka ayarları (`GET /api/v1/tenant/config`).
 * Host'tan çözülen tenant için anonim çağrılır; login ekranı da aynı kaynağı kullanır.
 */
export interface TenantConfig {
  tenantId: string
  subdomain: string
  name: string
  primaryColor: string
  logoUrl: string | null
  faviconUrl: string | null
}
