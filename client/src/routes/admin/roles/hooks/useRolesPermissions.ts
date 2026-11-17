import { useQueryClient } from '@tanstack/react-query'
import { App } from 'antd'
import { useMemo, useState } from 'react'
import { ApiError } from '@/core/api/apiError'
import { invalidate } from '@/core/api/queryClient'
import { useLanguage } from '@/core/context/useLanguage'
import { useAppMutation } from '@/core/hooks/useAppMutation'
import { useAppQuery } from '@/core/hooks/useAppQuery'
import { groupPermissionsByModule } from '@/core/permissionGroups'
import { queryKeys } from '@/services/queryKeys'
import { rbacQueries, rbacService } from '@/services/rbacService'
import { userQueries } from '@/services/userService'
import type { RoleDetail } from '@/types/rbac'
import type { UserItem } from '@/types/user'

/** Roller / izin matrisi ve kullanıcı rol atama durumu. */
export function useRolesPermissions() {
  const { t } = useLanguage()
  const { message } = App.useApp()
  const queryClient = useQueryClient()

  const rolesQuery = useAppQuery(rbacQueries.roles())
  const permissionsQuery = useAppQuery(rbacQueries.permissions())
  const usersQuery = useAppQuery(userQueries.list({ pageNumber: 1, pageSize: 100 }))

  const roles = rolesQuery.data ?? []
  const users = usersQuery.data?.items ?? []

  const [selectedRoleId, setSelectedRoleId] = useState<string | null>(null)
  const [draftPermissions, setDraftPermissions] = useState<string[] | null>(null)
  const [isEditing, setIsEditing] = useState(false)
  const [userSearch, setUserSearch] = useState('')

  const selectedRole = roles.find((role) => role.id === selectedRoleId) ?? roles[0] ?? null
  const activePermissions =
    isEditing && draftPermissions !== null ? draftPermissions : (selectedRole?.permissions ?? [])
  const moduleGroups = useMemo(
    () => groupPermissionsByModule(permissionsQuery.data ?? []),
    [permissionsQuery.data],
  )
  const isAdminRole = selectedRole?.name.toLowerCase() === 'admin'

  const selectRole = (role: RoleDetail) => {
    setSelectedRoleId(role.id)
    setDraftPermissions(role.permissions)
    setIsEditing(false)
  }

  const savePermissions = useAppMutation({
    mutationFn: () => {
      if (!selectedRole) return Promise.reject(new Error('role missing'))
      return rbacService.updateRolePermissions(selectedRole.id, { permissions: activePermissions })
    },
    onSuccess: async (updated) => {
      message.success(t('rbac.permissionsSaved', updated.name))
      setIsEditing(false)
      setDraftPermissions(updated.permissions)
      await invalidate(queryClient, queryKeys.roles)
    },
    onError: (error) => {
      message.error(error instanceof ApiError ? error.message : t('rbac.permissionsSaveFailed'))
    },
  })

  const assignRoles = useAppMutation({
    mutationFn: ({ user, roles: nextRoles }: { user: UserItem; roles: string[] }) =>
      rbacService.assignUserRoles(user.id, { roles: nextRoles }),
    onSuccess: async (updated) => {
      message.success(t('rbac.userRolesSaved', updated.email))
      await invalidate(queryClient, queryKeys.users)
    },
    onError: (error) => {
      message.error(error instanceof ApiError ? error.message : t('rbac.userRolesSaveFailed'))
    },
  })

  const toggleUserRole = (user: UserItem, roleName: string) => {
    const hasRole = user.roles.includes(roleName)
    const next = hasRole ? user.roles.filter((r) => r !== roleName) : [...user.roles, roleName]
    if (next.length === 0) {
      message.error(t('users.rolesRequired'))
      return
    }
    assignRoles.mutate({ user, roles: next })
  }

  const filteredUsers = users.filter((user) => {
    const q = userSearch.trim().toLowerCase()
    if (!q) return true
    return (
      user.email.toLowerCase().includes(q) ||
      `${user.firstName} ${user.lastName}`.toLowerCase().includes(q)
    )
  })

  return {
    t,
    rolesQuery,
    permissionsQuery,
    usersQuery,
    roles,
    selectedRole,
    selectedRoleId: selectedRole?.id ?? null,
    activePermissions,
    moduleGroups,
    isAdminRole,
    isEditing,
    setIsEditing,
    setDraftPermissions,
    selectRole,
    savePermissions,
    assignRoles,
    toggleUserRole,
    userSearch,
    setUserSearch,
    filteredUsers,
    refreshing: rolesQuery.isFetching || permissionsQuery.isFetching || usersQuery.isFetching,
  }
}
