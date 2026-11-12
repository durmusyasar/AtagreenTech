import { ArrowRightOutlined } from '@ant-design/icons'
import { Flex, Segmented, Space, Table, Tag, Typography } from 'antd'
import { useState } from 'react'
import { useApiQuery } from '../../core/api/useApiQuery'
import { useLanguage } from '../../core/context/useLanguage'
import { stockService } from '../../services/stockService'
import type { MovementType, StockMovement } from '../../types/stock'
import type { Warehouse } from '../../types/warehouse'
import { MOVEMENT_TYPES } from './movementRequest'
import { MovementTypeTag } from './MovementTypeTag'
import { formatDateTime, formatPrice, formatQuantity } from './stockFormat'
import { WarehouseSelect } from './WarehouseSelect'

const PAGE_SIZE = 10

type TypeFilter = MovementType | 'all'

interface MovementHistoryProps {
  /** Filtre için tüm depolar (pasifler dahil). */
  warehouses: Warehouse[]
  /** Değiştikçe liste yeniden yüklenir (ör. yeni hareket kaydedildiğinde). */
  refreshKey: number
}

/** En yeni hareket en üstte; depo filtresi kaynağı veya hedefi o depo olan hareketleri getirir. */
export function MovementHistory({ warehouses, refreshKey }: MovementHistoryProps) {
  const { language, t } = useLanguage()
  const [pageNumber, setPageNumber] = useState(1)
  const [type, setType] = useState<TypeFilter>('all')
  const [warehouseId, setWarehouseId] = useState<string>()

  const { data, loading } = useApiQuery(
    (signal) =>
      stockService.listMovements(
        { pageNumber, pageSize: PAGE_SIZE, warehouseId, type: type === 'all' ? undefined : type },
        signal,
      ),
    [pageNumber, warehouseId, type, refreshKey],
  )

  return (
    <Space orientation="vertical" size="middle" style={{ width: '100%' }}>
      <Flex wrap gap={12} align="center">
        <Segmented<TypeFilter>
          value={type}
          onChange={(value) => {
            setPageNumber(1)
            setType(value)
          }}
          options={[
            { value: 'all', label: t('common.all') },
            ...MOVEMENT_TYPES.map((value) => ({ value, label: t(`movement.type.${value}`) })),
          ]}
        />
        <WarehouseSelect
          warehouses={warehouses}
          value={warehouseId}
          onChange={(value) => {
            setPageNumber(1)
            setWarehouseId(value)
          }}
          placeholder={t('movement.filterByWarehouse')}
          aria-label={t('movement.filterByWarehouse')}
          style={{ width: 280 }}
        />
      </Flex>

      <Table<StockMovement>
        rowKey="id"
        size="middle"
        loading={loading}
        dataSource={data?.items}
        scroll={{ x: 960 }}
        pagination={{
          current: pageNumber,
          pageSize: PAGE_SIZE,
          total: data?.totalCount,
          showSizeChanger: false,
          onChange: setPageNumber,
        }}
        columns={[
          {
            title: t('movement.date'),
            dataIndex: 'createdAt',
            width: 150,
            render: (createdAt: string) => formatDateTime(createdAt, language),
          },
          {
            title: t('movement.typeLabel'),
            dataIndex: 'type',
            width: 110,
            render: (value: MovementType) => <MovementTypeTag type={value} />,
          },
          {
            title: t('movement.product'),
            key: 'product',
            render: (_, movement) => (
              <Space orientation="vertical" size={0}>
                {movement.productName}
                <Typography.Text type="secondary" style={{ fontSize: 12 }}>
                  {movement.productSku}
                </Typography.Text>
              </Space>
            ),
          },
          {
            title: t('movement.warehouses'),
            key: 'warehouses',
            render: (_, movement) => (
              <Space size={4}>
                {movement.sourceWarehouseCode && <Tag>{movement.sourceWarehouseCode}</Tag>}
                {movement.sourceWarehouseCode && movement.targetWarehouseCode && <ArrowRightOutlined />}
                {movement.targetWarehouseCode && <Tag>{movement.targetWarehouseCode}</Tag>}
              </Space>
            ),
          },
          {
            title: t('movement.quantity'),
            dataIndex: 'quantity',
            align: 'right',
            width: 110,
            render: (quantity: number) => formatQuantity(quantity, language),
          },
          {
            title: t('movement.unitPrice'),
            dataIndex: 'unitPrice',
            align: 'right',
            width: 110,
            render: (unitPrice: number) => formatPrice(unitPrice, language),
          },
          {
            title: t('movement.document'),
            dataIndex: 'documentReference',
            width: 140,
            render: (reference: string | null) => reference ?? '—',
          },
          {
            title: t('movement.description'),
            dataIndex: 'description',
            ellipsis: true,
            render: (description: string | null) => description ?? '—',
          },
        ]}
      />
    </Space>
  )
}
