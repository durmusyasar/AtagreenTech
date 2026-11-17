import { PlusOutlined, ReloadOutlined } from '@ant-design/icons'
import { Button, Flex, Input, Segmented, Space, Typography } from 'antd'
import { useAuth } from '@/core/context/useAuth'
import { EditUserModal } from '@/routes/admin/users/components/EditUserModal'
import { InviteUserModal } from '@/routes/admin/users/components/InviteUserModal'
import { useUsersManagement } from '@/routes/admin/users/hooks/useUsersManagement'
import { UsersTable } from '@/routes/admin/users/list/UsersTable'
import type { StatusFilter } from '@/routes/admin/users/type'

export function UsersPage() {
  const { claims } = useAuth()
  const {
    pageSize,
    pageNumber,
    setPageNumber,
    status,
    inviteOpen,
    setInviteOpen,
    editor,
    setEditor,
    page,
    refresh,
    toggleActive,
    remove,
    filterSearch,
    filterStatus,
    t,
  } = useUsersManagement()

  return (
    <Space orientation="vertical" size="large" style={{ width: '100%' }}>
      <Flex justify="space-between" align="center" wrap gap={16}>
        <div>
          <Typography.Title level={3} style={{ margin: 0 }}>
            {t('users.title')}
          </Typography.Title>
          <Typography.Text type="secondary">{t('users.description')}</Typography.Text>
        </div>
        <Space wrap>
          <Button icon={<ReloadOutlined />} onClick={() => void page.refetch()} loading={page.isFetching}>
            {t('common.refresh')}
          </Button>
          <Button type="primary" icon={<PlusOutlined />} onClick={() => setInviteOpen(true)}>
            {t('users.invite')}
          </Button>
        </Space>
      </Flex>

      <Flex gap={12} wrap>
        <Input.Search
          allowClear
          placeholder={t('users.search')}
          onSearch={filterSearch}
          style={{ maxWidth: 360, flex: 1 }}
        />
        <Segmented<StatusFilter>
          value={status}
          onChange={filterStatus}
          options={[
            { value: 'all', label: t('common.all') },
            { value: 'active', label: t('list.active') },
            { value: 'inactive', label: t('list.inactive') },
          ]}
        />
      </Flex>

      <UsersTable
        users={page.data?.items}
        loading={page.isPending}
        pageNumber={pageNumber}
        pageSize={pageSize}
        totalCount={page.data?.totalCount ?? 0}
        currentUserId={claims?.subject ?? undefined}
        onPageChange={setPageNumber}
        onEdit={(user) => setEditor({ open: true, user })}
        onToggleActive={(user) => toggleActive.mutate(user)}
        onDelete={(user) => remove.mutate(user)}
        toggleActivePendingId={toggleActive.isPending ? toggleActive.variables?.id : undefined}
        deletePendingId={remove.isPending ? remove.variables?.id : undefined}
      />

      <InviteUserModal
        open={inviteOpen}
        onClose={() => setInviteOpen(false)}
        onSuccess={() => void refresh()}
      />

      <EditUserModal
        open={editor.open}
        user={editor.user}
        onClose={() => setEditor((current) => ({ ...current, open: false }))}
        onSuccess={() => void refresh()}
      />
    </Space>
  )
}
