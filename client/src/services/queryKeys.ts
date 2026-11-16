/**
 * Sorgu anahtarlarının kökleri. Bir alanın tüm sorguları kendi kökünün altındadır; kayıttan sonra kök
 * bayatlatılarak o alandaki bütün listeler tazelenir.
 */
export const queryKeys = {
  session: ['session'],
  /** Host'tan çözülen tenant'ın logo / renk / favicon ayarları. */
  tenantConfig: ['tenant', 'config'],
  users: ['users'],
  unitsOfMeasure: ['units-of-measure'],
  categories: ['categories'],
  products: ['products'],
  warehouses: ['warehouses'],
  /** Hareketler ve bakiyeler birlikte değişir. */
  stock: ['stock'],
} as const
