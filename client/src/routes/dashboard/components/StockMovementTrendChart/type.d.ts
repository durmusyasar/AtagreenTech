import type { StockTrendPoint } from '@/routes/dashboard/type'

export interface StockMovementTrendChartProps {
  data: StockTrendPoint[]
  isLoading: boolean
}
