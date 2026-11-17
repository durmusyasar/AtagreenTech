import type { SelectProps } from 'antd'
import type { Product } from '@/types/product'

export type ProductOption = { value: string; label: string; product: Product }

export type ProductSelectProps = Omit<
  SelectProps<string, ProductOption>,
  'options' | 'showSearch' | 'optionRender' | 'filterOption' | 'loading' | 'notFoundContent'
> & {
  /** Yalnızca aktif ürünler; stok hareketi pasif ürünle yapılamaz. */
  activeOnly?: boolean
  /** Seçilen ürünün tamamı (ör. ölçü birimi ve birim fiyat için); seçim temizlenince `undefined`. */
  onProductChange?: (product: Product | undefined) => void
}
