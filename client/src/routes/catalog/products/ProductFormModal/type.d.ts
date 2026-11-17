import type { Category } from '@/types/category'
import type { Product } from '@/types/product'
import type { UnitOfMeasure } from '@/types/unitOfMeasure'

export interface ProductFormModalProps {
  open: boolean
  /** Düzenlenecek ürün; `null` ise yeni ürün oluşturulur. */
  product: Product | null
  /** Kategori seçeneklerinin kaynağı: tüm kategoriler. */
  categories: Category[]
  categoriesLoading: boolean
  /** Ölçü birimi seçeneklerinin kaynağı: tüm birimler. */
  units: UnitOfMeasure[]
  unitsLoading: boolean
  onClose: () => void
  onSaved: () => void
}
