import { ReloadOutlined, SafetyCertificateOutlined } from '@ant-design/icons'
import { useQueryClient } from '@tanstack/react-query'
import {
  App,
  Button,
  Card,
  Checkbox,
  Col,
  Flex,
  Input,
  Row,
  Space,
  Table,
  Tabs,
  Tag,
  Typography,
} from 'antd'
import { useMemo, useState } from 'react'
import { ApiError } from '../../core/api/apiError'
import { invalidate } from '../../core/api/queryClient'
import { useLanguage } from '../../core/context/useLanguage'
import { useAppMutation } from '../../core/hooks/useAppMutation'
import { useAppQuery } from '../../core/hooks/useAppQuery'
import { hasMessage, type MessageKey } from '../../core/i18n/messages'
import { groupPermissionsByModule } from '../../core/permissionGroups'
import { queryKeys } from '../../services/queryKeys'
import { rbacQueries, rbacService } from '../../services/rbacService'
import { userQueries } from '../../services/userService'
import type { RoleDetail } from '../../types/rbac'
import type { UserItem } from '../../types/user'

export function RolesPage() {
  const { t } = useLanguage()
  const { message } = App.useApp()
  const queryClient = useQueryClient()

  const rolesQuery = useAppQuery(rbacQueries.roles())
  const permissionsQuery = useAppQuery(rbacQueries.permissions())
  const usersQuery = useAppQuery(
    userQueries.list({ pageNumber: 1, pageSize: 100 }),
  )

  const roles = rolesQuery.data ?? []
  const users = usersQuery.data?.items ?? []

  const [selectedRoleId, setSelectedRoleId] = useState<string | null>(null)
  const [draftPermissions, setDraftPermissions] = useState<string[] | null>(null)
  const [isEditing, setIsEditing] = useState(false)
  const [userSearch, setUserSearch] = useState('')

  const selectedRole =
    roles.find((role) => role.id === selectedRoleId) ?? roles[0] ?? null
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

  const refreshing = rolesQuery.isFetching || permissionsQuery.isFetching || usersQuery.isFetching

  return (
    <Space orientation="vertical" size="large" style={{ width: '100%' }}>
      <Flex justify="space-between" align="center" wrap gap={16}>
        <div>
          <Typography.Title level={3} style={{ margin: 0 }}>
            {t('rbac.title')}
          </Typography.Title>
          <Typography.Text type="secondary">{t('rbac.description')}</Typography.Text>
        </div>
        <Button
          icon={<ReloadOutlined />}
          loading={refreshing}
          onClick={() => {
            void rolesQuery.refetch()
            void permissionsQuery.refetch()
            void usersQuery.refetch()
          }}
        >
          {t('common.refresh')}
        </Button>
      </Flex>

      <Tabs
        items={[
          {
            key: 'matrix',
            label: t('rbac.tabMatrix'),
            children: (
              <Row gutter={[16, 16]}>
                <Col xs={24} lg={8}>
                  <Space orientation="vertical" style={{ width: '100%' }} size="middle">
                    <Typography.Text type="secondary" style={{ textTransform: 'uppercase', fontSize: 12 }}>
                      {t('rbac.rolesHeading')}
                    </Typography.Text>
                    {roles.map((role) => {
                      const selected = role.id === selectedRole?.id
                      return (
                        <Card
                          key={role.id}
                          size="small"
                          hoverable
                          onClick={() => selectRole(role)}
                          style={{
                            borderColor: selected ? 'var(--ant-color-primary)' : undefined,
                            cursor: 'pointer',
                          }}
                        >
                          <Flex justify="space-between" align="center" gap={8}>
                            <Typography.Text strong>{role.name}</Typography.Text>
                            <Tag>
                              {role.permissions.length} {t('rbac.permissionCount')}
                            </Tag>
                          </Flex>
                          <Typography.Paragraph type="secondary" style={{ marginBottom: 0, marginTop: 8 }}>
                            {role.description || '—'}
                          </Typography.Paragraph>
                        </Card>
                      )
                    })}
                  </Space>
                </Col>

                <Col xs={24} lg={16}>
                  <Card
                    loading={rolesQuery.isPending || permissionsQuery.isPending}
                    title={
                      selectedRole
                        ? t('rbac.rolePermissionsTitle', selectedRole.name)
                        : t('rbac.selectRole')
                    }
                    extra={
                      selectedRole && !isAdminRole ? (
                        isEditing ? (
                          <Space>
                            <Button
                              onClick={() => {
                                setDraftPermissions(selectedRole.permissions)
                                setIsEditing(false)
                              }}
                            >
                              {t('common.cancel')}
                            </Button>
                            <Button
                              type="primary"
                              loading={savePermissions.isPending}
                              onClick={() => savePermissions.mutate()}
                            >
                              {t('common.save')}
                            </Button>
                          </Space>
                        ) : (
                          <Button
                            icon={<SafetyCertificateOutlined />}
                            onClick={() => {
                              setDraftPermissions(selectedRole.permissions)
                              setIsEditing(true)
                            }}
                          >
                            {t('rbac.editPermissions')}
                          </Button>
                        )
                      ) : null
                    }
                  >
                    {selectedRole ? (
                      <>
                        <Typography.Paragraph type="secondary">
                          {isAdminRole
                            ? t('rbac.adminLocked')
                            : isEditing
                              ? t('rbac.editModeHint')
                              : t('rbac.readModeHint')}
                        </Typography.Paragraph>

                        <Space orientation="vertical" size="large" style={{ width: '100%' }}>
                          {moduleGroups.map((group) => {
                            const labelKey = `rbac.module.${group.moduleKey}`
                            const label = hasMessage(labelKey)
                              ? t(labelKey as MessageKey)
                              : group.moduleKey.toUpperCase()
                            return (
                              <div key={group.moduleKey}>
                                <Typography.Text
                                  type="secondary"
                                  style={{ textTransform: 'uppercase', fontSize: 12 }}
                                >
                                  {label}
                                </Typography.Text>
                                <Row gutter={[12, 12]} style={{ marginTop: 8 }}>
                                  {group.items.map((perm) => {
                                    const checked = activePermissions.includes(perm.name)
                                    return (
                                      <Col xs={24} sm={12} key={perm.id}>
                                        <Card
                                          size="small"
                                          style={{
                                            opacity: checked ? 1 : 0.65,
                                            borderColor: checked
                                              ? 'var(--ant-color-success)'
                                              : undefined,
                                          }}
                                        >
                                          <Checkbox
                                            checked={checked}
                                            disabled={!isEditing || isAdminRole}
                                            onChange={() => {
                                              if (!isEditing || isAdminRole) return
                                              setDraftPermissions((prev) => {
                                                const current = prev ?? selectedRole.permissions
                                                return current.includes(perm.name)
                                                  ? current.filter((p) => p !== perm.name)
                                                  : [...current, perm.name]
                                              })
                                            }}
                                          >
                                            <div>
                                              <Typography.Text code>{perm.name}</Typography.Text>
                                              <div>
                                                <Typography.Text type="secondary" style={{ fontSize: 12 }}>
                                                  {perm.description}
                                                </Typography.Text>
                                              </div>
                                            </div>
                                          </Checkbox>
                                        </Card>
                                      </Col>
                                    )
                                  })}
                                </Row>
                              </div>
                            )
                          })}
                        </Space>
                      </>
                    ) : (
                      <Typography.Text type="secondary">{t('rbac.selectRole')}</Typography.Text>
                    )}
                  </Card>
                </Col>
              </Row>
            ),
          },
          {
            key: 'assign',
            label: t('rbac.tabAssign'),
            children: (
              <Card>
                <Input.Search
                  allowClear
                  placeholder={t('rbac.userSearch')}
                  onSearch={setUserSearch}
                  onChange={(e) => {
                    if (!e.target.value) setUserSearch('')
                  }}
                  style={{ maxWidth: 360, marginBottom: 16 }}
                />

                <Table<UserItem>
                  rowKey="id"
                  loading={usersQuery.isPending}
                  dataSource={filteredUsers}
                  pagination={false}
                  columns={[
                    {
                      title: t('users.columnUser'),
                      key: 'user',
                      render: (_, user) => (
                        <div>
                          <div>
                            {user.firstName} {user.lastName}
                          </div>
                          <Typography.Text type="secondary" style={{ fontSize: 12 }}>
                            {user.email}
                          </Typography.Text>
                        </div>
                      ),
                    },
                    ...roles.map((role) => ({
                      title: role.name,
                      key: role.id,
                      align: 'center' as const,
                      render: (_: unknown, user: UserItem) => {
                        const assigned = user.roles.includes(role.name)
                        const busy =
                          assignRoles.isPending && assignRoles.variables?.user.id === user.id
                        return (
                          <Button
                            size="small"
                            type={assigned ? 'primary' : 'default'}
                            ghost={assigned}
                            loading={busy}
                            onClick={() => toggleUserRole(user, role.name)}
                          >
                            {assigned ? t('rbac.assigned') : t('rbac.assign')}
                          </Button>
                        )
                      },
                    })),
                  ]}
                />
              </Card>
            ),
          },
        ]}
      />
    </Space>
  )
}
