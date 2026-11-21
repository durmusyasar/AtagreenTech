import { z } from 'zod'
import type { ListPageFilterBase } from '@/Components/ListPage/type'

/** Varsayılan liste filtresi — yalnızca serbest metin arama. */
export const defaultListFilterSchema: z.ZodType<ListPageFilterBase> = z.object({
  search: z.string(),
})

export const defaultListFilterValues: ListPageFilterBase = {
  search: '',
}
