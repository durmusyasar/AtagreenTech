import type { MovementType } from '../../../types/stock'
import type { Warehouse } from '../../../types/warehouse'

export type TypeFilter = MovementType | 'all'

export interface MovementHistoryProps {
  /** Filtre için tüm depolar (pasifler dahil). */
  warehouses: Warehouse[]
}
