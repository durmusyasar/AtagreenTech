import { Alert, Input, Space, Table, Typography } from 'antd'
import { useState } from 'react'
import { StatusTag } from '../../components/StatusTag'
import { TranslationTags } from '../../components/TranslationTags'
import { useApiQuery } from '../../core/api/useApiQuery'
import { useLanguage } from '../../core/context/useLanguage'
import { DEFAULT_LANGUAGE } from '../../core/i18n/languages'
import { categoryService } from '../../services/categoryService'
import type { Category } from '../../types/category'

const PAGE_SIZE = 20

export function CategoriesPage() {
  const { language, t } = useLanguage()
  const [pageNumber, setPageNumber] = useState(1)
  const [searchTerm, setSearchTerm] = useState<string>()

  const { data, loading } = useApiQuery(
    (signal) => categoryService.list({ language, pageNumber, pageSize: PAGE_SIZE, searchTerm }, signal),
    [language, pageNumber, searchTerm],
  )

  return (
    <Space orientation="vertical" size="large" style={{ width: '100%' }}>
      <Typography.Title level={3} style={{ margin: 0 }}>
        {t('menu.categories')}
      </Typography.Title>

      <Alert type="info" showIcon title={t('list.readOnlyNotice', 'Issue #11')} />

      <Input.Search
        allowClear
        placeholder={t('list.search')}
        style={{ maxWidth: 360 }}
        onSearch={(value) => {
          setPageNumber(1)
          setSearchTerm(value.trim() || undefined)
        }}
      />

      <Table<Category>
        rowKey="id"
        loading={loading}
        dataSource={data?.items}
        pagination={{
          current: pageNumber,
          pageSize: PAGE_SIZE,
          total: data?.totalCount,
          showSizeChanger: false,
          onChange: setPageNumber,
        }}
        columns={[
          { title: t('list.code'), dataIndex: 'code', width: 160 },
          { title: t('list.name'), dataIndex: 'name' },
          { title: t('list.description'), dataIndex: 'description', ellipsis: true },
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
        ]}
      />
    </Space>
  )
}
