import type { SystemMenuListItem } from '@/services/systemMenusService'

export interface MenuFormDrawerProps {
  open: boolean
  menu: SystemMenuListItem | null
  onClose: () => void
  onSaved: () => void
}

export interface MenuFormValues {
  code: string
  moduleId: string
  parentId?: string | null
  routePath: string
  iconKey: string
  requiredPermission?: string
  displayOrder: number
  isActive: boolean
  translations: Record<string, { name?: string }>
}
