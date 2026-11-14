import { keepPreviousData, useQuery } from '@tanstack/react-query'
import { Flex, Select, Spin, Tag, Typography, type SelectProps } from 'antd'
import { useEffect, useMemo, useState } from 'react'
import { useLanguage } from '../../core/context/useLanguage'
import { productQueries } from '../../services/productService'
import type { Product } from '../../types/product'

const SEARCH_DELAY_MS = 300
const RESULT_LIMIT = 20

type ProductOption = { value: string; label: string; product: Product }

type ProductSelectProps = Omit<
  SelectProps<string, ProductOption>,
  'options' | 'showSearch' | 'optionRender' | 'filterOption' | 'loading' | 'notFoundContent'
> & {
  /** Yalnızca aktif ürünler; stok hareketi pasif ürünle yapılamaz. */
  activeOnly?: boolean
  /** Seçilen ürünün tamamı (ör. ölçü birimi ve birim fiyat için); seçim temizlenince `undefined`. */
  onProductChange?: (product: Product | undefined) => void
}

/**
 * Ürünler sunucuda ad veya SKU ile aranır; katalog büyük olabileceğinden tümü yüklenmez, ilk {@link RESULT_LIMIT}
 * sonuç gösterilir.
 */
export function ProductSelect({ activeOnly = false, onProductChange, onChange, ...props }: ProductSelectProps) {
  const { t } = useLanguage()
  const [search, setSearch] = useState('')
  const [searchTerm, setSearchTerm] = useState<string>()
  // Seçili ürün, sonraki aramaların sonuçlarında olmasa da etiketiyle görünmeye devam etsin diye ayrıca tutulur.
  const [selected, setSelected] = useState<Product>()

  useEffect(() => {
    const timer = setTimeout(() => setSearchTerm(search.trim() || undefined), SEARCH_DELAY_MS)
    return () => clearTimeout(timer)
  }, [search])

  // Yeni aramanın sonucu gelene kadar önceki sonuçlar listede kalır.
  const { data, isFetching: loading } = useQuery({
    ...productQueries.list({ searchTerm, isActive: activeOnly ? true : undefined, pageSize: RESULT_LIMIT }),
    placeholderData: keepPreviousData,
  })

  const options = useMemo(() => {
    const products = data?.items ?? []
    const all = selected && !products.some((product) => product.id === selected.id) ? [selected, ...products] : products
    return all.map((product) => ({ value: product.id, label: `${product.name} · ${product.sku}`, product }))
  }, [data, selected])

  return (
    <Select<string, ProductOption>
      allowClear
      showSearch={{ filterOption: false, onSearch: setSearch }}
      options={options}
      loading={loading}
      notFoundContent={loading ? <Spin size="small" /> : undefined}
      optionRender={({ data: option }) => (
        <Flex justify="space-between" align="center" gap={8}>
          <Typography.Text ellipsis>{option.product.name}</Typography.Text>
          <span>
            {!option.product.isActive && <Tag>{t('list.inactive')}</Tag>}
            <Typography.Text type="secondary" code>
              {option.product.sku}
            </Typography.Text>
          </span>
        </Flex>
      )}
      onChange={(value, option) => {
        const product = options.find((candidate) => candidate.value === value)?.product
        setSelected(product)
        onProductChange?.(product)
        onChange?.(value, option)
      }}
      {...props}
    />
  )
}
