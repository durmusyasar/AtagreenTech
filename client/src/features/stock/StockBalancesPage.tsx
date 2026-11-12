import { ReloadOutlined } from '@ant-design/icons'
import { Button, Flex, Space, Switch, Table, Tag, Typography } from 'antd'
import { useState } from 'react'
import { useApiQuery } from '../../core/api/useApiQuery'
import { useLanguage } from '../../core/context/useLanguage'
import { stockService } from '../../services/stockService'
import { warehouseService } from '../../services/warehouseService'
import type { StockBalance } from '../../types/stock'
import { ProductSelect } from './ProductSelect'
import { formatDateTime, formatQuantity } from './stockFormat'
import { WarehouseSelect } from './WarehouseSelect'

const PAGE_SIZE = 20

/** Depo bazında anlık stok; bakiyeler yalnızca stok hareketleriyle değişir. */
export function StockBalancesPage() {
  const { language, t } = useLanguage()
  const [pageNumber, setPageNumber] = useState(1)
  const [warehouseId, setWarehouseId] = useState<string>()
  const [productId, setProductId] = useState<string>()
  const [includeEmpty, setIncludeEmpty] = useState(false)

  const warehouses = useApiQuery((signal) => warehouseService.listAll({}, signal), [])
  const balances = useApiQuery(
    (signal) =>
      stockService.listBalances({ pageNumber, pageSize: PAGE_SIZE, warehouseId, productId, includeEmpty }, signal),
    [pageNumber, warehouseId, productId, includeEmpty],
  )

  // Filtre değişince ilk sayfaya dönülür; aksi halde sonuç az olduğunda boş bir sayfada kalınabilir.
  const filter =
    <T,>(set: (value: T) => void) =>
    (value: T) => {
      setPageNumber(1)
      set(value)
    }

  return (
    <Space orientation="vertical" size="large" style={{ width: '100%' }}>
      <Flex justify="space-between" align="center" wrap gap={16}>
        <Typography.Title level={3} style={{ margin: 0 }}>
          {t('menu.stockBalances')}
        </Typography.Title>
        <Button icon={<ReloadOutlined />} onClick={balances.reload}>
          {t('common.refresh')}
        </Button>
      </Flex>

      <Flex wrap gap={12} align="center">
        <WarehouseSelect
          warehouses={warehouses.data ?? []}
          loading={warehouses.loading}
          value={warehouseId}
          onChange={filter(setWarehouseId)}
          placeholder={t('movement.filterByWarehouse')}
          aria-label={t('movement.filterByWarehouse')}
          style={{ width: 280 }}
        />
        <ProductSelect
          value={productId}
          onChange={filter(setProductId)}
          placeholder={t('balance.filterByProduct')}
          aria-label={t('balance.filterByProduct')}
          style={{ width: 320 }}
        />
        <Space>
          <Switch checked={includeEmpty} onChange={filter(setIncludeEmpty)} aria-label={t('balance.includeEmpty')} />
          {t('balance.includeEmpty')}
        </Space>
      </Flex>

      <Table<StockBalance>
        rowKey={(balance) => `${balance.warehouseId}:${balance.productId}`}
        loading={balances.loading}
        dataSource={balances.data?.items}
        pagination={{
          current: pageNumber,
          pageSize: PAGE_SIZE,
          total: balances.data?.totalCount,
          showSizeChanger: false,
          onChange: setPageNumber,
        }}
        columns={[
          {
            title: t('balance.warehouse'),
            key: 'warehouse',
            render: (_, balance) => (
              <Space>
                <Tag>{balance.warehouseCode}</Tag>
                {balance.warehouseName}
              </Space>
            ),
          },
          {
            title: t('movement.product'),
            key: 'product',
            render: (_, balance) => (
              <Space orientation="vertical" size={0}>
                {balance.productName}
                <Typography.Text type="secondary" style={{ fontSize: 12 }}>
                  {balance.productSku}
                </Typography.Text>
              </Space>
            ),
          },
          {
            title: t('balance.quantity'),
            key: 'quantity',
            align: 'right',
            width: 180,
            render: (_, balance) => (
              <Typography.Text strong type={balance.quantity === 0 ? 'secondary' : undefined}>
                {formatQuantity(balance.quantity, language)} {balance.unitOfMeasureCode}
              </Typography.Text>
            ),
          },
          {
            title: t('balance.lastChangedAt'),
            dataIndex: 'lastChangedAt',
            width: 170,
            render: (value: string) => formatDateTime(value, language),
          },
        ]}
      />
    </Space>
  )
}
