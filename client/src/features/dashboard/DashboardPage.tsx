import { CheckCircleTwoTone, ReloadOutlined } from '@ant-design/icons'
import { Button, Card, Descriptions, Space, Tag, Typography } from 'antd'
import { useApiQuery } from '../../core/api/useApiQuery'
import { useAuth } from '../../core/context/useAuth'
import { useLanguage } from '../../core/context/useLanguage'
import { sessionService } from '../../services/sessionService'

export function DashboardPage() {
  const { language, t } = useLanguage()
  const { claims } = useAuth()
  const { data, loading, reload } = useApiQuery((signal) => sessionService.getCurrent(signal), [])

  const expiresAt = claims?.expiresAt ? new Date(claims.expiresAt * 1000).toLocaleString(language) : '—'

  return (
    <Space orientation="vertical" size="large" style={{ width: '100%' }}>
      <Typography.Title level={3} style={{ margin: 0 }}>
        {t('dashboard.title')}
      </Typography.Title>

      <Card
        title={t('dashboard.connection')}
        loading={loading && !data}
        extra={
          <Button icon={<ReloadOutlined />} onClick={reload} loading={loading}>
            {t('dashboard.refresh')}
          </Button>
        }
      >
        {data && (
          <Descriptions column={1} bordered size="small">
            <Descriptions.Item label={t('dashboard.connection')}>
              <Space>
                <CheckCircleTwoTone twoToneColor="#52c41a" />
                {t('dashboard.connected')}
              </Space>
            </Descriptions.Item>
            <Descriptions.Item label={t('dashboard.user')}>{data.email ?? data.userId ?? '—'}</Descriptions.Item>
            <Descriptions.Item label={t('dashboard.roles')}>
              {data.roles.map((role) => (
                <Tag key={role}>{role}</Tag>
              ))}
            </Descriptions.Item>
            <Descriptions.Item label={t('dashboard.tenant')}>{data.tenant ?? '—'}</Descriptions.Item>
            <Descriptions.Item label={t('dashboard.tokenExpires')}>{expiresAt}</Descriptions.Item>
          </Descriptions>
        )}
      </Card>
    </Space>
  )
}
