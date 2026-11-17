import type { Warehouse } from '@/types/warehouse'

export interface MovementFormProps {
  /** Hareket yalnızca aktif depolarla yapılabilir. */
  warehouses: Warehouse[]
}
