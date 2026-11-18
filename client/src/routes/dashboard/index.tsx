import { CheckCircleTwoTone, ReloadOutlined } from '@ant-design/icons'
import { useQuery } from '@tanstack/react-query'
import { Button, Descriptions, Space, Tag, Typography } from 'antd'
import { useAuth } from '@/core/context/useAuth'
import { useLanguage } from '@/core/context/useLanguage'
import { sessionQueries } from '@/services/sessionService'

export function DashboardPage() {
  const { language, t } = useLanguage()
  const { claims } = useAuth()
  const { data, isPending, isFetching, refetch } = useQuery(sessionQueries.current())

  const expiresAt = claims?.expiresAt
    ? new Date(claims.expiresAt * 1000).toLocaleString(language)
    : '—'

  return (
    <Space orientation="vertical" size="large" className="w-full">
      <Typography.Title level={3} className="!m-0 text-primary">
        {t('dashboard.title')}
      </Typography.Title>

      <section className="overflow-hidden rounded-2xl border border-subtle bg-surface">
        <div className="flex items-center justify-between gap-3 border-b border-subtle px-4 py-3 sm:px-5">
          <h2 className="text-sm font-semibold text-primary">{t('dashboard.connection')}</h2>
          <Button icon={<ReloadOutlined />} onClick={() => refetch()} loading={isFetching}>
            {t('common.refresh')}
          </Button>
        </div>

        <div className="p-4 sm:p-5">
          {isPending ? (
            <p className="text-sm text-secondary">{t('login.verifying')}</p>
          ) : data ? (
            <Descriptions column={1} bordered size="small">
              <Descriptions.Item label={t('dashboard.connection')}>
                <Space>
                  <CheckCircleTwoTone twoToneColor="#52c41a" />
                  {t('dashboard.connected')}
                </Space>
              </Descriptions.Item>
              <Descriptions.Item label={t('dashboard.user')}>
                {data.email || data.username || '—'}
              </Descriptions.Item>
              <Descriptions.Item label={t('dashboard.roles')}>
                {data.roles.map((role) => (
                  <Tag key={role}>{role}</Tag>
                ))}
              </Descriptions.Item>
              <Descriptions.Item label={t('dashboard.tenant')}>
                {data.tenantId || '—'}
              </Descriptions.Item>
              <Descriptions.Item label={t('dashboard.tokenExpires')}>{expiresAt}</Descriptions.Item>
            </Descriptions>
          ) : null}
        </div>
      </section>
    </Space>
  )
}
