import { z } from 'zod'

/** Tenant listesi filtre şeması — satır DTO’sundan ayrı; yalnızca API query alanları. */
export const tenantsFilterSchema = z.object({
  search: z.string(),
})

export type TenantsFilterValues = z.infer<typeof tenantsFilterSchema>

export const tenantsFilterDefaults: TenantsFilterValues = {
  search: '',
}
