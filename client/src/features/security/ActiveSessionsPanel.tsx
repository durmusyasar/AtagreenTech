import { DisconnectOutlined, ReloadOutlined } from '@ant-design/icons'
import { useQueryClient } from '@tanstack/react-query'
import { App, Button, Flex, Popconfirm, Space, Table, Tag, Typography } from 'antd'
import { invalidate } from '../../core/api/queryClient'
import { useLanguage } from '../../core/context/useLanguage'
import { useAppMutation } from '../../core/hooks/useAppMutation'
import { useAppQuery } from '../../core/hooks/useAppQuery'
import { queryKeys } from '../../services/queryKeys'
import { accountSecurityQueries, accountSecurityService } from '../../services/accountSecurityService'
import type { ActiveSessionItem } from '../../types/security'

export function ActiveSessionsPanel() {
  const { language, t } = useLanguage()
  const { message } = App.useApp()
  const queryClient = useQueryClient()
  const sessions = useAppQuery(accountSecurityQueries.sessions())

  const refresh = () => invalidate(queryClient, queryKeys.sessions)

  const revokeOne = useAppMutation({
    mutationFn: (id: string) => accountSecurityService.revokeSession(id),
    onSuccess: async () => {
      message.success(t('security.sessionRevoked'))
      await refresh()
    },
  })

  const revokeOthers = useAppMutation({
    mutationFn: () => accountSecurityService.revokeOtherSessions(),
    onSuccess: async () => {
      message.success(t('security.otherSessionsRevoked'))
      await refresh()
    },
  })

  const rows = sessions.data ?? []
  const hasOthers = rows.some((session) => !session.isCurrent)

  return (
    <Space orientation="vertical" size="middle" style={{ width: '100%' }}>
      <Flex justify="space-between" align="flex-start" wrap gap={12}>
        <div>
          <Typography.Title level={4} style={{ margin: 0 }}>
            {t('security.sessionsTitle')}
          </Typography.Title>
          <Typography.Text type="secondary">{t('security.sessionsDescription')}</Typography.Text>
        </div>
        <Space wrap>
          <Button
            icon={<ReloadOutlined />}
            onClick={() => void sessions.refetch()}
            loading={sessions.isFetching}
          >
            {t('common.refresh')}
          </Button>
          {hasOthers && (
            <Popconfirm
              title={t('security.revokeOthersConfirm')}
              okText={t('security.revokeOthers')}
              cancelText={t('common.cancel')}
              onConfirm={() => revokeOthers.mutate()}
            >
              <Button danger icon={<DisconnectOutlined />} loading={revokeOthers.isPending}>
                {t('security.revokeOthers')}
              </Button>
            </Popconfirm>
          )}
        </Space>
      </Flex>

      <Table<ActiveSessionItem>
        rowKey="id"
        loading={sessions.isPending}
        dataSource={rows}
        pagination={false}
        columns={[
          {
            title: t('security.device'),
            key: 'device',
            render: (_, session) => (
              <Space wrap>
                <span>{session.deviceInfo || t('security.unknownDevice')}</span>
                {session.isCurrent && <Tag color="success">{t('security.currentSession')}</Tag>}
              </Space>
            ),
          },
          {
            title: t('security.ip'),
            dataIndex: 'ipAddress',
            width: 140,
            render: (ip: string | null) => ip || '—',
          },
          {
            title: t('security.signedIn'),
            dataIndex: 'createdAt',
            width: 180,
            render: (value: string) => new Date(value).toLocaleString(language),
          },
          {
            title: t('security.expires'),
            dataIndex: 'expiresAt',
            width: 180,
            render: (value: string) => new Date(value).toLocaleString(language),
          },
          {
            title: t('users.actions'),
            key: 'actions',
            align: 'right',
            width: 140,
            render: (_, session) =>
              session.isCurrent ? null : (
                <Button
                  size="small"
                  danger
                  loading={revokeOne.isPending && revokeOne.variables === session.id}
                  onClick={() => revokeOne.mutate(session.id)}
                >
                  {t('security.revokeOne')}
                </Button>
              ),
          },
        ]}
      />
    </Space>
  )
}
