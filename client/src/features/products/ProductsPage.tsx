import { DeleteOutlined, EditOutlined, PlusOutlined, ReloadOutlined } from '@ant-design/icons'
import { keepPreviousData, useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { App, Button, Flex, Input, Popconfirm, Segmented, Space, Table, Tag, Typography } from 'antd'
import { useMemo, useState } from 'react'
import { StatusTag } from '../../components/StatusTag'
import { TranslationTags } from '../../components/TranslationTags'
import { invalidate } from '../../core/api/queryClient'
import { useLanguage } from '../../core/context/useLanguage'
import { DEFAULT_LANGUAGE } from '../../core/i18n/languages'
import { categoryQueries } from '../../services/categoryService'
import { productQueries, productService } from '../../services/productService'
import { queryKeys } from '../../services/queryKeys'
import { unitOfMeasureQueries } from '../../services/unitOfMeasureService'
import type { Product } from '../../types/product'
import { parentOptions } from '../categories/categoryForm'
import { ParentCategorySelect } from '../categories/ParentCategorySelect'
import { formatMoney } from '../stock/stockFormat'
import { ProductFormModal } from './ProductFormModal'

const PAGE_SIZE = 20

type StatusFilter = 'all' | 'active' | 'inactive'

export function ProductsPage() {
  const { language, t } = useLanguage()
  const { message } = App.useApp()
  const [pageNumber, setPageNumber] = useState(1)
  const [searchTerm, setSearchTerm] = useState<string>()
  const [status, setStatus] = useState<StatusFilter>('all')
  const [categoryId, setCategoryId] = useState<string>()
  // Kapanış animasyonu bitene kadar başlık ve form değişmesin diye düzenlenen ürün modal kapanınca da tutulur.
  const [editor, setEditor] = useState<{ open: boolean; product: Product | null }>({ open: false, product: null })

  const queryClient = useQueryClient()
  const page = useQuery({
    ...productQueries.list({
      language,
      pageNumber,
      pageSize: PAGE_SIZE,
      searchTerm,
      categoryId,
      isActive: status === 'all' ? undefined : status === 'active',
    }),
    placeholderData: keepPreviousData,
  })
  // Kategori filtresi ve formdaki seçimler için tüm kategoriler ve birimler; adlar arayüz diline göre çözümlenir.
  const categories = useQuery(categoryQueries.listAll(language))
  const units = useQuery(unitOfMeasureQueries.list({ language }))
  const categoryOptions = useMemo(() => parentOptions(categories.data ?? []), [categories.data])

  // Silme hataları (ör. stok hareketi olan ürün) global bildirimde gösterilir.
  const remove = useMutation({
    mutationFn: (product: Product) => productService.remove(product.id),
    onSuccess: (_, product) => {
      message.success(t('product.deleted', product.sku))
      return invalidate(queryClient, queryKeys.products)
    },
  })

  // Filtre değişince ilk sayfaya dönülür; aksi halde sonuç az olduğunda boş bir sayfada kalınabilir.
  const filter =
    <T,>(set: (value: T) => void) =>
    (value: T) => {
      setPageNumber(1)
      set(value)
    }

  const openEditor = (product: Product | null) => setEditor({ open: true, product })
  const closeEditor = () => setEditor((current) => ({ ...current, open: false }))

  return (
    <Space orientation="vertical" size="large" style={{ width: '100%' }}>
      <Flex justify="space-between" align="center" wrap gap={16}>
        <Typography.Title level={3} style={{ margin: 0 }}>
          {t('menu.products')}
        </Typography.Title>
        <Space wrap>
          <Button icon={<ReloadOutlined />} onClick={() => invalidate(queryClient, queryKeys.products)}>
            {t('common.refresh')}
          </Button>
          <Button type="primary" icon={<PlusOutlined />} onClick={() => openEditor(null)}>
            {t('product.create')}
          </Button>
        </Space>
      </Flex>

      <Flex wrap gap={12} align="center">
        <Input.Search
          allowClear
          placeholder={t('product.search')}
          style={{ width: 280 }}
          onSearch={(value) => filter(setSearchTerm)(value.trim() || undefined)}
        />
        <ParentCategorySelect
          options={categoryOptions}
          loading={categories.isFetching}
          value={categoryId}
          onChange={filter(setCategoryId)}
          placeholder={t('product.filterByCategory')}
          aria-label={t('product.filterByCategory')}
          style={{ width: 280 }}
        />
        <Segmented<StatusFilter>
          value={status}
          onChange={filter(setStatus)}
          options={[
            { value: 'all', label: t('common.all') },
            { value: 'active', label: t('list.active') },
            { value: 'inactive', label: t('list.inactive') },
          ]}
        />
      </Flex>

      <Table<Product>
        rowKey="id"
        loading={page.isFetching}
        dataSource={page.data?.items}
        scroll={{ x: 1100 }}
        pagination={{
          current: pageNumber,
          pageSize: PAGE_SIZE,
          total: page.data?.totalCount,
          showSizeChanger: false,
          onChange: setPageNumber,
        }}
        columns={[
          { title: t('product.sku'), dataIndex: 'sku', width: 160, render: (sku: string) => <Tag>{sku}</Tag> },
          {
            title: t('product.barcode'),
            dataIndex: 'barcode',
            width: 150,
            render: (barcode: string | null) => (barcode ? <Typography.Text code>{barcode}</Typography.Text> : '—'),
          },
          {
            title: t('list.name'),
            key: 'name',
            render: (_, product) => (
              <Space orientation="vertical" size={0}>
                {product.name}
                {product.description && (
                  <Typography.Text type="secondary" ellipsis style={{ fontSize: 12, maxWidth: 320 }}>
                    {product.description}
                  </Typography.Text>
                )}
              </Space>
            ),
          },
          { title: t('product.category'), dataIndex: 'categoryName', width: 180 },
          {
            title: t('product.unitOfMeasure'),
            dataIndex: 'unitOfMeasureCode',
            width: 90,
            render: (code: string) => <Tag>{code}</Tag>,
          },
          {
            title: t('product.unitCost'),
            key: 'unitPrice',
            align: 'right',
            width: 140,
            render: (_, product) => formatMoney(product.unitPrice, product.currency, language),
          },
          {
            title: t('list.languages'),
            key: 'languages',
            width: 110,
            render: (_, product) => (
              <TranslationTags
                languages={product.translations.map((translation) => translation.languageCode)}
                defaultLanguage={DEFAULT_LANGUAGE}
              />
            ),
          },
          {
            title: t('list.status'),
            dataIndex: 'isActive',
            width: 100,
            render: (isActive: boolean) => <StatusTag isActive={isActive} />,
          },
          {
            key: 'actions',
            width: 200,
            align: 'right',
            fixed: 'right',
            render: (_, product) => (
              <Space size={0}>
                <Button type="link" icon={<EditOutlined />} onClick={() => openEditor(product)}>
                  {t('common.edit')}
                </Button>
                <Popconfirm
                  title={t('product.deleteTitle', product.sku)}
                  description={t('product.deleteDescription')}
                  okText={t('common.delete')}
                  cancelText={t('common.cancel')}
                  okButtonProps={{ danger: true }}
                  onConfirm={() => remove.mutateAsync(product).catch(() => undefined)}
                >
                  <Button
                    type="link"
                    danger
                    icon={<DeleteOutlined />}
                    loading={remove.isPending && remove.variables?.id === product.id}
                  >
                    {t('common.delete')}
                  </Button>
                </Popconfirm>
              </Space>
            ),
          },
        ]}
      />

      <ProductFormModal
        open={editor.open}
        product={editor.product}
        categories={categories.data ?? []}
        categoriesLoading={categories.isFetching}
        units={units.data ?? []}
        unitsLoading={units.isFetching}
        onClose={closeEditor}
        onSaved={closeEditor}
      />
    </Space>
  )
}
