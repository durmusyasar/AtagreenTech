import type { CategoryDistributionItem } from '@/routes/dashboard/type'

export interface WarehouseDistributionChartProps {
  data: CategoryDistributionItem[]
  isLoading: boolean
}
