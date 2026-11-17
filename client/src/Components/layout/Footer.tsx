import { Layout, theme, Typography } from 'antd'
import { APP_VERSION } from '@/core/appVersion'
import { useLanguage } from '@/core/context/useLanguage'
import { useTenantTheme } from '@/core/context/useTenantTheme'

/** Modül yükleme yılı; render sırasında `Date` çağrılmasın diye sabitlenir. */
const COPYRIGHT_YEAR = new Date().getFullYear()

/** Telif ve sürüm bilgisi; tüm oturum açık sayfalarda `AppLayout` altında gösterilir. */
export function Footer() {
  const { t } = useLanguage()
  const { config } = useTenantTheme()
  const { token } = theme.useToken()

  return (
    <Layout.Footer
      style={{
        textAlign: 'center',
        padding: '16px 24px',
        background: token.colorBgLayout,
        color: token.colorTextSecondary,
      }}
    >
      <Typography.Text type="secondary">
        {t('footer.line', config.name, COPYRIGHT_YEAR, APP_VERSION)}
      </Typography.Text>
    </Layout.Footer>
  )
}
