import type { StockTrendPoint } from '@modules/dashboard/types'

export interface StockMovementTrendChartProps {
  data: StockTrendPoint[]
  isLoading: boolean
}
