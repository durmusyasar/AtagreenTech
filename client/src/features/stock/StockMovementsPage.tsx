import { ReloadOutlined } from '@ant-design/icons'
import { useQuery, useQueryClient } from '@tanstack/react-query'
import { Button, Card, Flex, Space, Typography } from 'antd'
import { useMemo } from 'react'
import { invalidate } from '../../core/api/queryClient'
import { useLanguage } from '../../core/context/useLanguage'
import { queryKeys } from '../../services/queryKeys'
import { warehouseQueries } from '../../services/warehouseService'
import { MovementForm } from './MovementForm'
import { MovementHistory } from './MovementHistory'

/** Stok giriş, çıkış ve transfer konsolu; altında hareket geçmişi. */
export function StockMovementsPage() {
  const { t } = useLanguage()
  const queryClient = useQueryClient()
  const warehouses = useQuery(warehouseQueries.listAll())
  const activeWarehouses = useMemo(() => (warehouses.data ?? []).filter((warehouse) => warehouse.isActive), [warehouses.data])

  return (
    <Space orientation="vertical" size="large" style={{ width: '100%' }}>
      <Flex justify="space-between" align="center" wrap gap={16}>
        <Typography.Title level={3} style={{ margin: 0 }}>
          {t('menu.stockMovements')}
        </Typography.Title>
        <Button icon={<ReloadOutlined />} onClick={() => invalidate(queryClient, queryKeys.warehouses, queryKeys.stock)}>
          {t('common.refresh')}
        </Button>
      </Flex>

      <Card title={t('movement.newTitle')} loading={warehouses.isPending}>
        <MovementForm warehouses={activeWarehouses} />
      </Card>

      <Card title={t('movement.historyTitle')}>
        <MovementHistory warehouses={warehouses.data ?? []} />
      </Card>
    </Space>
  )
}
