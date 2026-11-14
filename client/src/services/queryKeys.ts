/**
 * Sorgu anahtarlarının kökleri. Bir alanın tüm sorguları kendi kökünün altındadır; kayıttan sonra kök
 * bayatlatılarak o alandaki bütün listeler tazelenir.
 */
export const queryKeys = {
  session: ['session'],
  unitsOfMeasure: ['units-of-measure'],
  categories: ['categories'],
  products: ['products'],
  warehouses: ['warehouses'],
  /** Hareketler ve bakiyeler birlikte değişir. */
  stock: ['stock'],
} as const
