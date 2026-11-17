import type { UnitOfMeasure } from '../../../types/unitOfMeasure'

export interface UnitOfMeasureFormModalProps {
  open: boolean
  /** Düzenlenecek birim; `null` ise yeni birim oluşturulur. */
  unit: UnitOfMeasure | null
  onClose: () => void
  onSaved: () => void
}
