import type { CreateTenantRequest } from '@modules/system/api/tenantsService'

export interface TenantCreateModalProps {
  open: boolean
  confirming: boolean
  onClose: () => void
  onSubmit: (body: CreateTenantRequest) => void
}
