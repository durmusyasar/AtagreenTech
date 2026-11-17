import type { PermissionModuleGroup } from '@/core/permissionGroups'
import type { RoleDetail } from '@/types/rbac'
import type { UserItem } from '@/types/user'

export interface RoleListSidebarProps {
  roles: RoleDetail[]
  selectedRoleId: string | null
  onSelect: (role: RoleDetail) => void
}

export interface RolePermissionMatrixProps {
  selectedRole: RoleDetail | null
  moduleGroups: PermissionModuleGroup[]
  activePermissions: string[]
  isEditing: boolean
  isAdminRole: boolean
  loading: boolean
  saving: boolean
  onStartEdit: () => void
  onCancelEdit: () => void
  onSave: () => void
  onTogglePermission: (permissionName: string) => void
}

export interface UserRoleAssignTableProps {
  users: UserItem[]
  roles: RoleDetail[]
  loading: boolean
  assigningUserId?: string
  onToggleUserRole: (user: UserItem, roleName: string) => void
}
