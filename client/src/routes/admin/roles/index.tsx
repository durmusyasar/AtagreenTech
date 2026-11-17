import { ReloadOutlined } from '@ant-design/icons'
import { Button, Card, Col, Flex, Input, Row, Space, Table, Tabs, Typography } from 'antd'
import { RolePermissionMatrix } from '@/routes/admin/roles/detail/RolePermissionMatrix'
import { useRolesPermissions } from '@/routes/admin/roles/hooks/useRolesPermissions'
import { RoleListSidebar } from '@/routes/admin/roles/list/RoleListSidebar'
import type { UserItem } from '@/types/user'

export function RolesPage() {
  const {
    t,
    rolesQuery,
    permissionsQuery,
    usersQuery,
    roles,
    selectedRole,
    selectedRoleId,
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
    setUserSearch,
    filteredUsers,
    refreshing,
  } = useRolesPermissions()

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
                  <RoleListSidebar
                    roles={roles}
                    selectedRoleId={selectedRoleId}
                    onSelect={selectRole}
                  />
                </Col>
                <Col xs={24} lg={16}>
                  <RolePermissionMatrix
                    selectedRole={selectedRole}
                    moduleGroups={moduleGroups}
                    activePermissions={activePermissions}
                    isEditing={isEditing}
                    isAdminRole={isAdminRole}
                    loading={rolesQuery.isPending || permissionsQuery.isPending}
                    saving={savePermissions.isPending}
                    onStartEdit={() => {
                      if (!selectedRole) return
                      setDraftPermissions(selectedRole.permissions)
                      setIsEditing(true)
                    }}
                    onCancelEdit={() => {
                      if (!selectedRole) return
                      setDraftPermissions(selectedRole.permissions)
                      setIsEditing(false)
                    }}
                    onSave={() => savePermissions.mutate()}
                    onTogglePermission={(permissionName) => {
                      if (!selectedRole) return
                      setDraftPermissions((prev) => {
                        const current = prev ?? selectedRole.permissions
                        return current.includes(permissionName)
                          ? current.filter((p) => p !== permissionName)
                          : [...current, permissionName]
                      })
                    }}
                  />
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
