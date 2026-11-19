import type { CategoryDistributionItem } from '@modules/dashboard/types'

export interface WarehouseDistributionChartProps {
  data: CategoryDistributionItem[]
  isLoading: boolean
}
