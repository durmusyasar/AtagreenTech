import { z } from 'zod'

/** Tenant oluşturma formu — Zod alan doğrulaması (i18n anahtarlı mesajlar). */
export const tenantCreateSchema = z.object({
  name: z.string().trim().min(1, 'tenants.form.nameRequired').max(128),
  slug: z
    .string()
    .trim()
    .min(1, 'tenants.form.slugRequired')
    .regex(/^[a-z0-9]([a-z0-9-]{0,61}[a-z0-9])?$/, 'tenants.form.slugInvalid'),
  adminEmail: z
    .string()
    .trim()
    .refine((value) => value === '' || z.string().email().safeParse(value).success, {
      message: 'tenants.form.adminEmailInvalid',
    }),
})

export type TenantCreateSchemaValues = z.infer<typeof tenantCreateSchema>
