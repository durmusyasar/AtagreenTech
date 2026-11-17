import type { Warehouse } from '../../../types/warehouse'

export interface WarehouseFormModalProps {
  open: boolean
  /** Düzenlenecek depo; `null` ise yeni depo oluşturulur. */
  warehouse: Warehouse | null
  onClose: () => void
  onSaved: () => void
}
