/**
 * Sorgu anahtarlarının kökleri. Bir alanın tüm sorguları kendi kökünün altındadır; kayıttan sonra kök
 * bayatlatılarak o alandaki bütün listeler tazelenir.
 */
export const queryKeys = {
  session: ['session'],
  /** Aktif cihaz oturumları (`GET /sessions`). */
  sessions: ['sessions'],
  /** Host'tan çözülen tenant'ın logo / renk / favicon ayarları. */
  tenantConfig: ['tenant', 'config'],
  /** Tenant + izin filtreli dinamik menü (`GET /navigation/menus`). */
  navigation: ['navigation'],
  users: ['users'],
  roles: ['roles'],
  permissions: ['permissions'],
  unitsOfMeasure: ['units-of-measure'],
  categories: ['categories'],
  products: ['products'],
  warehouses: ['warehouses'],
  /** Hareketler ve bakiyeler birlikte değişir. */
  stock: ['stock'],
} as const
