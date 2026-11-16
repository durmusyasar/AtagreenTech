import { queryOptions } from '@tanstack/react-query'
import { axiosClient } from '../core/api/axiosClient'
import type { AssignUserRolesRequest, PermissionItem, RoleDetail, UpdateRolePermissionsRequest } from '../types/rbac'
import type { UserItem } from '../types/user'
import { queryKeys } from './queryKeys'

export const rbacService = {
  async getRoles(signal?: AbortSignal): Promise<RoleDetail[]> {
    const { data } = await axiosClient.get<RoleDetail[]>('/roles', { signal })
    return data
  },

  async getPermissions(signal?: AbortSignal): Promise<PermissionItem[]> {
    const { data } = await axiosClient.get<PermissionItem[]>('/permissions', { signal })
    return data
  },

  async updateRolePermissions(roleId: string, payload: UpdateRolePermissionsRequest): Promise<RoleDetail> {
    const { data } = await axiosClient.put<RoleDetail>(
      `/roles/${roleId}/permissions`,
      { permissions: payload.permissions },
      { skipErrorNotification: true },
    )
    return data
  },

  async assignUserRoles(userId: string, payload: AssignUserRolesRequest): Promise<UserItem> {
    const { data } = await axiosClient.put<UserItem>(
      `/users/${userId}/roles`,
      { roles: payload.roles },
      { skipErrorNotification: true },
    )
    return data
  },
}

export const rbacQueries = {
  roles: () =>
    queryOptions({
      queryKey: [...queryKeys.roles, 'list'],
      queryFn: ({ signal }) => rbacService.getRoles(signal),
    }),
  permissions: () =>
    queryOptions({
      queryKey: [...queryKeys.permissions, 'list'],
      queryFn: ({ signal }) => rbacService.getPermissions(signal),
    }),
}
