import type { ColumnsType } from 'antd/es/table'
import type { ListColumnPreference } from '@/Components/ListPage/type'

export interface ListColumnSettingsDrawerProps<TRecord extends object> {
  open: boolean
  columns: ColumnsType<TRecord>
  preferences: ListColumnPreference[]
  onClose: () => void
  onSave: (preferences: ListColumnPreference[]) => void
  onReset: () => void
}
