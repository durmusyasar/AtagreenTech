import { EditOutlined, PlusOutlined, ReloadOutlined } from '@ant-design/icons'
import { keepPreviousData, useQuery, useQueryClient } from '@tanstack/react-query'
import { Button, Flex, Input, Segmented, Space, Table, Tag, Typography } from 'antd'
import { useMemo, useState } from 'react'
import { StatusTag } from '../../components/StatusTag'
import { TranslationTags } from '../../components/TranslationTags'
import { invalidate } from '../../core/api/queryClient'
import { useLanguage } from '../../core/context/useLanguage'
import { DEFAULT_LANGUAGE } from '../../core/i18n/languages'
import { categoryQueries } from '../../services/categoryService'
import { queryKeys } from '../../services/queryKeys'
import type { Category } from '../../types/category'
import { parentOptions } from './categoryForm'
import { CategoryFormModal } from './CategoryFormModal'
import { ParentCategorySelect } from './ParentCategorySelect'

const PAGE_SIZE = 20

type StatusFilter = 'all' | 'active' | 'inactive'

export function CategoriesPage() {
  const { language, t } = useLanguage()
  const [pageNumber, setPageNumber] = useState(1)
  const [searchTerm, setSearchTerm] = useState<string>()
  const [status, setStatus] = useState<StatusFilter>('all')
  const [parentCategoryId, setParentCategoryId] = useState<string>()
  // Kapanış animasyonu bitene kadar başlık ve form değişmesin diye düzenlenen kategori modal kapanınca da tutulur.
  const [editor, setEditor] = useState<{ open: boolean; category: Category | null }>({ open: false, category: null })

  const queryClient = useQueryClient()
  const page = useQuery({
    ...categoryQueries.list({
      language,
      pageNumber,
      pageSize: PAGE_SIZE,
      searchTerm,
      parentCategoryId,
      isActive: status === 'all' ? undefined : status === 'active',
    }),
    placeholderData: keepPreviousData,
  })
  // Üst kategori filtresi ve formdaki seçim için tüm kategoriler; adlar arayüz diline göre çözümlenir.
  const all = useQuery(categoryQueries.listAll(language))
  const allOptions = useMemo(() => parentOptions(all.data ?? []), [all.data])

  // Filtre değişince ilk sayfaya dönülür; aksi halde sonuç az olduğunda boş bir sayfada kalınabilir.
  const filter =
    <T,>(set: (value: T) => void) =>
    (value: T) => {
      setPageNumber(1)
      set(value)
    }

  const openEditor = (category: Category | null) => setEditor({ open: true, category })
  const closeEditor = () => setEditor((current) => ({ ...current, open: false }))

  return (
    <Space orientation="vertical" size="large" style={{ width: '100%' }}>
      <Flex justify="space-between" align="center" wrap gap={16}>
        <Typography.Title level={3} style={{ margin: 0 }}>
          {t('menu.categories')}
        </Typography.Title>
        <Space wrap>
          <Button icon={<ReloadOutlined />} onClick={() => invalidate(queryClient, queryKeys.categories)}>
            {t('common.refresh')}
          </Button>
          <Button type="primary" icon={<PlusOutlined />} onClick={() => openEditor(null)}>
            {t('category.create')}
          </Button>
        </Space>
      </Flex>

      <Flex wrap gap={12} align="center">
        <Input.Search
          allowClear
          placeholder={t('list.search')}
          style={{ width: 280 }}
          onSearch={(value) => filter(setSearchTerm)(value.trim() || undefined)}
        />
        <ParentCategorySelect
          options={allOptions}
          loading={all.isFetching}
          value={parentCategoryId}
          onChange={filter(setParentCategoryId)}
          placeholder={t('category.filterByParent')}
          aria-label={t('category.filterByParent')}
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

      <Table<Category>
        rowKey="id"
        loading={page.isFetching}
        dataSource={page.data?.items}
        pagination={{
          current: pageNumber,
          pageSize: PAGE_SIZE,
          total: page.data?.totalCount,
          showSizeChanger: false,
          onChange: setPageNumber,
        }}
        columns={[
          { title: t('list.code'), dataIndex: 'code', width: 180, render: (code: string) => <Tag>{code}</Tag> },
          { title: t('list.name'), dataIndex: 'name' },
          {
            title: t('list.description'),
            dataIndex: 'description',
            ellipsis: true,
            render: (description: string | null) => description ?? '—',
          },
          { title: t('list.parent'), dataIndex: 'parentCategoryName', render: (name: string | null) => name ?? '—' },
          {
            title: t('list.languages'),
            key: 'languages',
            render: (_, category) => (
              <TranslationTags
                languages={category.translations.map((translation) => translation.languageCode)}
                defaultLanguage={DEFAULT_LANGUAGE}
              />
            ),
          },
          {
            title: t('list.status'),
            dataIndex: 'isActive',
            width: 120,
            render: (isActive: boolean) => <StatusTag isActive={isActive} />,
          },
          {
            key: 'actions',
            width: 120,
            align: 'right',
            render: (_, category) => (
              <Button type="link" icon={<EditOutlined />} onClick={() => openEditor(category)}>
                {t('common.edit')}
              </Button>
            ),
          },
        ]}
      />

      <CategoryFormModal
        open={editor.open}
        category={editor.category}
        categories={all.data ?? []}
        categoriesLoading={all.isFetching}
        onClose={closeEditor}
        onSaved={closeEditor}
      />
    </Space>
  )
}
