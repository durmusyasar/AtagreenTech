import { SafetyCertificateOutlined } from '@ant-design/icons'
import { Button, Card, Checkbox, Col, Row, Space, Typography } from 'antd'
import { useLanguage } from '@/core/context/useLanguage'
import { hasMessage, type MessageKey } from '@/core/i18n/messages'
import type { RolePermissionMatrixProps } from '@/routes/admin/roles/type'

export function RolePermissionMatrix({
  selectedRole,
  moduleGroups,
  activePermissions,
  isEditing,
  isAdminRole,
  loading,
  saving,
  onStartEdit,
  onCancelEdit,
  onSave,
  onTogglePermission,
}: RolePermissionMatrixProps) {
  const { t } = useLanguage()

  return (
    <Card
      loading={loading}
      title={selectedRole ? t('rbac.rolePermissionsTitle', selectedRole.name) : t('rbac.selectRole')}
      extra={
        selectedRole && !isAdminRole ? (
          isEditing ? (
            <Space>
              <Button onClick={onCancelEdit}>{t('common.cancel')}</Button>
              <Button type="primary" loading={saving} onClick={onSave}>
                {t('common.save')}
              </Button>
            </Space>
          ) : (
            <Button icon={<SafetyCertificateOutlined />} onClick={onStartEdit}>
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
              const label = hasMessage(labelKey) ? t(labelKey as MessageKey) : group.moduleKey.toUpperCase()
              return (
                <div key={group.moduleKey}>
                  <Typography.Text type="secondary" style={{ textTransform: 'uppercase', fontSize: 12 }}>
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
                              borderColor: checked ? 'var(--ant-color-success)' : undefined,
                            }}
                          >
                            <Checkbox
                              checked={checked}
                              disabled={!isEditing || isAdminRole}
                              onChange={() => {
                                if (!isEditing || isAdminRole) return
                                onTogglePermission(perm.name)
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
  )
}
