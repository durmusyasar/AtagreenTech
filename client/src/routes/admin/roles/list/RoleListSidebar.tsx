import { Card, Flex, Space, Tag, Typography } from 'antd'
import { useLanguage } from '@/core/context/useLanguage'
import type { RoleListSidebarProps } from '@/routes/admin/roles/type'

export function RoleListSidebar({ roles, selectedRoleId, onSelect }: RoleListSidebarProps) {
  const { t } = useLanguage()

  return (
    <Space orientation="vertical" style={{ width: '100%' }} size="middle">
      <Typography.Text type="secondary" style={{ textTransform: 'uppercase', fontSize: 12 }}>
        {t('rbac.rolesHeading')}
      </Typography.Text>
      {roles.map((role) => {
        const selected = role.id === selectedRoleId
        return (
          <Card
            key={role.id}
            size="small"
            hoverable
            onClick={() => onSelect(role)}
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
  )
}
