import { CheckCircleTwoTone, ReloadOutlined } from '@ant-design/icons'
import { useQuery } from '@tanstack/react-query'
import { Button, Card, Descriptions, Space, Tag, Typography } from 'antd'
import { useAuth } from '../../core/context/useAuth'
import { useLanguage } from '../../core/context/useLanguage'
import { sessionQueries } from '../../services/sessionService'

export function DashboardPage() {
  const { language, t } = useLanguage()
  const { claims } = useAuth()
  const { data, isPending, isFetching, refetch } = useQuery(sessionQueries.current())

  const expiresAt = claims?.expiresAt ? new Date(claims.expiresAt * 1000).toLocaleString(language) : '—'

  return (
    <Space orientation="vertical" size="large" style={{ width: '100%' }}>
      <Typography.Title level={3} style={{ margin: 0 }}>
        {t('dashboard.title')}
      </Typography.Title>

      <Card
        title={t('dashboard.connection')}
        loading={isPending}
        extra={
          <Button icon={<ReloadOutlined />} onClick={() => refetch()} loading={isFetching}>
            {t('common.refresh')}
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
