import type { Category } from '../../../types/category'

export interface CategoryFormModalProps {
  open: boolean
  /** Düzenlenecek kategori; `null` ise yeni kategori oluşturulur. */
  category: Category | null
  /** Üst kategori seçeneklerinin kaynağı: tüm kategoriler. */
  categories: Category[]
  categoriesLoading: boolean
  onClose: () => void
  onSaved: () => void
}
