/** `GET /api/v1/permissions` satırı. */
export interface PermissionItem {
  id: string
  name: string
  description: string
}

/** `GET /api/v1/roles` satırı. */
export interface RoleDetail {
  id: string
  name: string
  description: string
  permissions: string[]
}

export interface UpdateRolePermissionsRequest {
  permissions: string[]
}

export interface AssignUserRolesRequest {
  roles: string[]
}
