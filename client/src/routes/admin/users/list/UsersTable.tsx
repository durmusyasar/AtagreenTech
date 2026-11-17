import {
  CheckOutlined,
  DeleteOutlined,
  EditOutlined,
  StopOutlined,
} from '@ant-design/icons'
import { Button, Popconfirm, Space, Table, Tag, Typography } from 'antd'
import { StatusTag } from '@/Components/StatusTag'
import { useLanguage } from '@/core/context/useLanguage'
import type { UserItem } from '@/types/user'
import type { UsersTableProps } from '@/routes/admin/users/type'

export function UsersTable({
  users,
  loading,
  pageNumber,
  pageSize,
  totalCount,
  currentUserId,
  onPageChange,
  onEdit,
  onToggleActive,
  onDelete,
  toggleActivePendingId,
  deletePendingId,
}: UsersTableProps) {
  const { t } = useLanguage()

  return (
    <Table<UserItem>
      rowKey="id"
      loading={loading}
      dataSource={users}
      pagination={{
        current: pageNumber,
        pageSize,
        total: totalCount,
        showSizeChanger: false,
        onChange: onPageChange,
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
            const isSelf = currentUserId === user.id
            return (
              <Space size={4} wrap>
                <Button size="small" icon={<EditOutlined />} onClick={() => onEdit(user)}>
                  {t('common.edit')}
                </Button>
                {!isSelf && (
                  <>
                    <Button
                      size="small"
                      icon={user.isActive ? <StopOutlined /> : <CheckOutlined />}
                      loading={toggleActivePendingId === user.id}
                      onClick={() => onToggleActive(user)}
                    >
                      {user.isActive ? t('users.deactivate') : t('users.activate')}
                    </Button>
                    <Popconfirm
                      title={t('users.deleteConfirm', user.email)}
                      okText={t('common.delete')}
                      cancelText={t('common.cancel')}
                      onConfirm={() => onDelete(user)}
                    >
                      <Button
                        size="small"
                        danger
                        icon={<DeleteOutlined />}
                        loading={deletePendingId === user.id}
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
  )
}
