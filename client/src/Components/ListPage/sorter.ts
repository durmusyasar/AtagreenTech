import type { SorterResult } from 'antd/es/table/interface'
import type { ListPageSorter } from '@/Components/ListPage/type'

/** Ant Design Table sorter sonucunu API meta şekline çevirir. */
export function toListPageSorter<TRecord extends object>(
  sorter: SorterResult<TRecord> | SorterResult<TRecord>[],
): ListPageSorter | undefined {
  const single = Array.isArray(sorter) ? sorter[0] : sorter
  if (!single || !single.order) return undefined
  const field =
    typeof single.field === 'string'
      ? single.field
      : typeof single.columnKey === 'string'
        ? single.columnKey
        : undefined
  if (!field) return undefined
  return { field, order: single.order }
}

/** ListPageSorter → Ant Design `sortOrder` (sütun `sortOrder` prop). */
export function sortOrderForField(
  sorter: ListPageSorter | undefined,
  field: string,
): 'ascend' | 'descend' | undefined {
  if (!sorter || sorter.field !== field) return undefined
  return sorter.order
}
