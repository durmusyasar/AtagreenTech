import {
  DeleteOutlined,
  EditOutlined,
  PlusOutlined,
  ReloadOutlined,
  StopOutlined,
  CheckOutlined,
} from '@ant-design/icons'
import { keepPreviousData, useQueryClient } from '@tanstack/react-query'
import { App, Button, Flex, Input, Popconfirm, Segmented, Space, Table, Tag, Typography } from 'antd'
import { useState } from 'react'
import { StatusTag } from '../../../components/StatusTag'
import { invalidate } from '../../../core/api/queryClient'
import { useAuth } from '../../../core/context/useAuth'
import { useLanguage } from '../../../core/context/useLanguage'
import { useAppMutation } from '../../../core/hooks/useAppMutation'
import { useAppQuery } from '../../../core/hooks/useAppQuery'
import { queryKeys } from '../../../services/queryKeys'
import { userQueries, userService } from '../../../services/userService'
import type { UserItem } from '../../../types/user'
import { EditUserModal } from '../EditUserModal'
import { InviteUserModal } from '../InviteUserModal'
import type { StatusFilter } from './type'

const PAGE_SIZE = 15

export function UsersPage() {
  const { t } = useLanguage()
  const { message } = App.useApp()
  const { claims } = useAuth()
  const queryClient = useQueryClient()

  const [pageNumber, setPageNumber] = useState(1)
  const [searchTerm, setSearchTerm] = useState<string>()
  const [status, setStatus] = useState<StatusFilter>('all')
  const [inviteOpen, setInviteOpen] = useState(false)
  const [editor, setEditor] = useState<{ open: boolean; user: UserItem | null }>({
    open: false,
    user: null,
  })

  const page = useAppQuery({
    ...userQueries.list({
      pageNumber,
      pageSize: PAGE_SIZE,
      searchTerm,
      isActive: status === 'all' ? undefined : status === 'active',
    }),
    placeholderData: keepPreviousData,
  })

  const refresh = () => invalidate(queryClient, queryKeys.users)

  const toggleActive = useAppMutation({
    mutationFn: (user: UserItem) =>
      user.isActive ? userService.deactivate(user.id) : userService.activate(user.id),
    onSuccess: async (_, user) => {
      message.success(t(user.isActive ? 'users.deactivated' : 'users.activated', user.email))
      await refresh()
    },
  })

  const remove = useAppMutation({
    mutationFn: (user: UserItem) => userService.remove(user.id),
    onSuccess: async (_, user) => {
      message.success(t('users.deleted', user.email))
      await refresh()
    },
  })

  const filter =
    <T,>(set: (value: T) => void) =>
    (value: T) => {
      setPageNumber(1)
      set(value)
    }

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
          onSearch={filter(setSearchTerm)}
          style={{ maxWidth: 360, flex: 1 }}
        />
        <Segmented<StatusFilter>
          value={status}
          onChange={filter(setStatus)}
          options={[
            { value: 'all', label: t('common.all') },
            { value: 'active', label: t('list.active') },
            { value: 'inactive', label: t('list.inactive') },
          ]}
        />
      </Flex>

      <Table<UserItem>
        rowKey="id"
        loading={page.isPending}
        dataSource={page.data?.items}
        pagination={{
          current: pageNumber,
          pageSize: PAGE_SIZE,
          total: page.data?.totalCount ?? 0,
          showSizeChanger: false,
          onChange: setPageNumber,
        }}
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
          {
            title: t('users.roles'),
            dataIndex: 'roles',
            render: (roles: string[]) => (
              <Space size={[4, 4]} wrap>
                {roles.map((role) => (
                  <Tag key={role}>{role}</Tag>
                ))}
              </Space>
            ),
          },
          {
            title: t('list.status'),
            dataIndex: 'isActive',
            width: 110,
            render: (isActive: boolean) => <StatusTag isActive={isActive} />,
          },
          {
            title: t('users.verification'),
            dataIndex: 'isEmailVerified',
            width: 120,
            render: (verified: boolean) =>
              verified ? t('users.verified') : t('users.verificationPending'),
          },
          {
            title: t('users.actions'),
            key: 'actions',
            align: 'right',
            width: 280,
            render: (_, user) => {
              const isSelf = claims?.subject === user.id
              return (
                <Space size={4} wrap>
                  <Button
                    size="small"
                    icon={<EditOutlined />}
                    onClick={() => setEditor({ open: true, user })}
                  >
                    {t('common.edit')}
                  </Button>
                  {!isSelf && (
                    <>
                      <Button
                        size="small"
                        icon={user.isActive ? <StopOutlined /> : <CheckOutlined />}
                        loading={toggleActive.isPending && toggleActive.variables?.id === user.id}
                        onClick={() => toggleActive.mutate(user)}
                      >
                        {user.isActive ? t('users.deactivate') : t('users.activate')}
                      </Button>
                      <Popconfirm
                        title={t('users.deleteConfirm', user.email)}
                        okText={t('common.delete')}
                        cancelText={t('common.cancel')}
                        onConfirm={() => remove.mutate(user)}
                      >
                        <Button
                          size="small"
                          danger
                          icon={<DeleteOutlined />}
                          loading={remove.isPending && remove.variables?.id === user.id}
                        >
                          {t('common.delete')}
                        </Button>
                      </Popconfirm>
                    </>
                  )}
                </Space>
              )
            },
          },
        ]}
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
