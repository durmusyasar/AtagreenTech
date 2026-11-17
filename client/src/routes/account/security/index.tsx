import { LockOutlined, SafetyCertificateOutlined, LaptopOutlined } from '@ant-design/icons'
import { Space, Tabs, Typography } from 'antd'
import { useLanguage } from '@/core/context/useLanguage'
import { ActiveSessionsPanel } from '@/routes/account/security/components/ActiveSessionsPanel'
import { ChangePasswordPanel } from '@/routes/account/security/components/ChangePasswordPanel'
import { MfaSetupPanel } from '@/routes/account/security/components/MfaSetupPanel'

/** Self-servis güvenlik merkezi: oturumlar, şifre, MFA. */
export function SecurityPage() {
  const { t } = useLanguage()

  return (
    <Space orientation="vertical" size="large" style={{ width: '100%' }}>
      <div>
        <Typography.Title level={3} style={{ margin: 0 }}>
          {t('security.title')}
        </Typography.Title>
        <Typography.Text type="secondary">{t('security.description')}</Typography.Text>
      </div>

      <Tabs
        items={[
          {
            key: 'sessions',
            label: (
              <span>
                <LaptopOutlined /> {t('security.tabSessions')}
              </span>
            ),
            children: <ActiveSessionsPanel />,
          },
          {
            key: 'password',
            label: (
              <span>
                <LockOutlined /> {t('security.tabPassword')}
              </span>
            ),
            children: <ChangePasswordPanel />,
          },
          {
            key: 'mfa',
            label: (
              <span>
                <SafetyCertificateOutlined /> {t('security.tabMfa')}
              </span>
            ),
            children: <MfaSetupPanel />,
          },
        ]}
      />
    </Space>
  )
}
