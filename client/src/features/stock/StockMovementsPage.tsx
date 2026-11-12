import { ReloadOutlined } from '@ant-design/icons'
import { Button, Card, Flex, Space, Typography } from 'antd'
import { useMemo, useState } from 'react'
import { useApiQuery } from '../../core/api/useApiQuery'
import { useLanguage } from '../../core/context/useLanguage'
import { warehouseService } from '../../services/warehouseService'
import { MovementForm } from './MovementForm'
import { MovementHistory } from './MovementHistory'

/** Stok giriş, çıkış ve transfer konsolu; altında hareket geçmişi. */
export function StockMovementsPage() {
  const { t } = useLanguage()
  const [refreshKey, setRefreshKey] = useState(0)
  const warehouses = useApiQuery((signal) => warehouseService.listAll({}, signal), [])
  const activeWarehouses = useMemo(() => (warehouses.data ?? []).filter((warehouse) => warehouse.isActive), [warehouses.data])

  const refresh = () => {
    warehouses.reload()
    setRefreshKey((key) => key + 1)
  }

  return (
    <Space orientation="vertical" size="large" style={{ width: '100%' }}>
      <Flex justify="space-between" align="center" wrap gap={16}>
        <Typography.Title level={3} style={{ margin: 0 }}>
          {t('menu.stockMovements')}
        </Typography.Title>
        <Button icon={<ReloadOutlined />} onClick={refresh}>
          {t('common.refresh')}
        </Button>
      </Flex>

      <Card title={t('movement.newTitle')} loading={warehouses.loading && !warehouses.data}>
        <MovementForm warehouses={activeWarehouses} onSaved={() => setRefreshKey((key) => key + 1)} />
      </Card>

      <Card title={t('movement.historyTitle')}>
        <MovementHistory warehouses={warehouses.data ?? []} refreshKey={refreshKey} />
      </Card>
    </Space>
  )
}
