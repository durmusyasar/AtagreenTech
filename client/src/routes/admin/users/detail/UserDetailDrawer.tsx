import { Button, Drawer, Space, Tag, Typography } from 'antd'
import { useLanguage } from '@/core/context/useLanguage'
import type { UserDetailDrawerProps } from '@/routes/admin/users/type'

/** Seçili kullanıcı özeti; düzenleme modalına geçiş. */
export function UserDetailDrawer({ user, open, onClose, onEdit }: UserDetailDrawerProps) {
  const { t } = useLanguage()

  return (
    <Drawer
      open={open}
      onClose={onClose}
      title={user ? `${user.firstName} ${user.lastName}` : t('users.title')}
      width={360}
    >
      {user && (
        <Space orientation="vertical" size="middle" style={{ width: '100%' }}>
          <div>
            <Typography.Text type="secondary">{t('login.email')}</Typography.Text>
            <div>{user.email}</div>
          </div>
          <div>
            <Typography.Text type="secondary">{t('users.roles')}</Typography.Text>
            <div>
              <Space size={[4, 4]} wrap>
                {user.roles.map((role) => (
                  <Tag key={role}>{role}</Tag>
                ))}
              </Space>
            </div>
          </div>
          <Button
            type="primary"
            onClick={() => {
              onEdit(user)
              onClose()
            }}
          >
            {t('common.edit')}
          </Button>
        </Space>
      )}
    </Drawer>
  )
}
