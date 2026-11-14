import { EditOutlined, PlusOutlined, ReloadOutlined } from '@ant-design/icons'
import { keepPreviousData, useQuery } from '@tanstack/react-query'
import { Button, Flex, Input, Segmented, Space, Table, Tag, Typography } from 'antd'
import { useState } from 'react'
import { StatusTag } from '../../components/StatusTag'
import { useLanguage } from '../../core/context/useLanguage'
import { warehouseQueries } from '../../services/warehouseService'
import type { Warehouse } from '../../types/warehouse'
import { WarehouseFormModal } from './WarehouseFormModal'

const PAGE_SIZE = 20

type StatusFilter = 'all' | 'active' | 'inactive'

export function WarehousesPage() {
  const { t } = useLanguage()
  const [pageNumber, setPageNumber] = useState(1)
  const [searchTerm, setSearchTerm] = useState<string>()
  const [status, setStatus] = useState<StatusFilter>('all')
  // Kapanış animasyonu bitene kadar başlık ve form değişmesin diye düzenlenen depo modal kapanınca da tutulur.
  const [editor, setEditor] = useState<{ open: boolean; warehouse: Warehouse | null }>({ open: false, warehouse: null })

  const { data, isFetching, refetch } = useQuery({
    ...warehouseQueries.list({
      pageNumber,
      pageSize: PAGE_SIZE,
      searchTerm,
      isActive: status === 'all' ? undefined : status === 'active',
    }),
    placeholderData: keepPreviousData,
  })

  const openEditor = (warehouse: Warehouse | null) => setEditor({ open: true, warehouse })
  const closeEditor = () => setEditor((current) => ({ ...current, open: false }))

  return (
    <Space orientation="vertical" size="large" style={{ width: '100%' }}>
      <Flex justify="space-between" align="center" wrap gap={16}>
        <Typography.Title level={3} style={{ margin: 0 }}>
          {t('menu.warehouses')}
        </Typography.Title>
        <Space wrap>
          <Button icon={<ReloadOutlined />} onClick={() => refetch()}>
            {t('common.refresh')}
          </Button>
          <Button type="primary" icon={<PlusOutlined />} onClick={() => openEditor(null)}>
            {t('warehouse.create')}
          </Button>
        </Space>
      </Flex>

      <Flex wrap gap={12} align="center">
        <Input.Search
          allowClear
          placeholder={t('list.search')}
          style={{ width: 280 }}
          onSearch={(value) => {
            setPageNumber(1)
            setSearchTerm(value.trim() || undefined)
          }}
        />
        <Segmented<StatusFilter>
          value={status}
          onChange={(value) => {
            setPageNumber(1)
            setStatus(value)
          }}
          options={[
            { value: 'all', label: t('common.all') },
            { value: 'active', label: t('list.active') },
            { value: 'inactive', label: t('list.inactive') },
          ]}
        />
      </Flex>

      <Table<Warehouse>
        rowKey="id"
        loading={isFetching}
        dataSource={data?.items}
        pagination={{
          current: pageNumber,
          pageSize: PAGE_SIZE,
          total: data?.totalCount,
          showSizeChanger: false,
          onChange: setPageNumber,
        }}
        columns={[
          { title: t('list.code'), dataIndex: 'code', width: 180, render: (code: string) => <Tag>{code}</Tag> },
          { title: t('list.name'), dataIndex: 'name' },
          {
            title: t('warehouse.address'),
            dataIndex: 'address',
            ellipsis: true,
            render: (address: string | null) => address ?? '—',
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
            render: (_, warehouse) => (
              <Button type="link" icon={<EditOutlined />} onClick={() => openEditor(warehouse)}>
                {t('common.edit')}
              </Button>
            ),
          },
        ]}
      />

      <WarehouseFormModal
        open={editor.open}
        warehouse={editor.warehouse}
        onClose={closeEditor}
        onSaved={closeEditor}
      />
    </Space>
  )
}
