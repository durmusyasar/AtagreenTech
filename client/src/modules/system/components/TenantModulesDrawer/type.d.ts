import type { Dayjs } from 'dayjs'
import type {
  TenantListItem,
  TenantModuleItemRequest,
} from '@modules/system/api/tenantsService'

export interface TenantModulesDrawerProps {
  tenant: TenantListItem | null
  saving: boolean
  onClose: () => void
  onSave: (modules: TenantModuleItemRequest[]) => void
}

export interface TenantModuleDraftRow {
  moduleId: string
  code: string
  name: string
  isCore: boolean
  isEnabled: boolean
  expiresAtUtc: Dayjs | null
}
