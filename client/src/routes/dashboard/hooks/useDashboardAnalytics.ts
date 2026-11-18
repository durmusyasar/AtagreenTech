import { useState } from 'react'
import { useAuth } from '@/core/context/useAuth'
import { useLanguage } from '@/core/context/useLanguage'
import { usePermissions } from '@/core/hooks/usePermissions'
import { useAppQuery } from '@/core/hooks/useAppQuery'
import { useNavigationMenus } from '@/hooks/useNavigationMenus'
import type { DateRangeFilter } from '@/routes/dashboard/type'
import { dashboardQueries } from '@/services/dashboardService'
import { useNavigationStore } from '@/stores/navigationStore'

/** Tenant menü aboneliği + izinlere göre dashboard özetini yükler. */
export function useDashboardAnalytics() {
  const { language } = useLanguage()
  const { isAuthenticated } = useAuth()
  const access = usePermissions()
  const menusQuery = useNavigationMenus()
  const groups = useNavigationStore((s) => s.groups)
  const [range, setRange] = useState<DateRangeFilter>('7d')

  const menusReady = menusQuery.isSuccess || menusQuery.isError || groups.length > 0
  const stockSubscribed = groups.some((g) => g.serviceName === 'StockService')
  // Menü henüz yoksa stock.read ile iyimser yükle; abonelik cevabı gelince düzelir.
  const includeStock = access.can('stock.read') && (!menusReady || stockSubscribed)
  const includeUsers = access.can('users.read')

  const query = useAppQuery({
    ...dashboardQueries.summary(range, language, includeStock, includeUsers),
    enabled: isAuthenticated && (menusReady || access.can('stock.read') || includeUsers),
  })

  return { ...query, range, setRange, includeStock, includeUsers }
}
