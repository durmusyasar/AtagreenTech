import type { SelectProps } from 'antd'
import type { Warehouse } from '@/types/warehouse'

export type WarehouseOption = { value: string; label: string; warehouse: Warehouse }

export type WarehouseSelectProps = Omit<SelectProps<string, WarehouseOption>, 'options' | 'showSearch' | 'optionRender'> & {
  warehouses: readonly Warehouse[]
}
