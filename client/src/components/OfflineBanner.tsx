import { DisconnectOutlined } from '@ant-design/icons'
import { Alert } from 'antd'
import { useLanguage } from '../core/context/useLanguage'
import { useNetworkStatus } from '../core/hooks/useNetworkStatus'

/**
 * Tarayıcı çevrimdışıyken üstte kalıcı uyarı. Login dahil tüm uygulama ağacında gösterilir.
 * Yerel veri senkronu yok; yalnızca bağlantı durumunu bildirir.
 */
export function OfflineBanner() {
  const { isOnline } = useNetworkStatus()
  const { t } = useLanguage()

  if (isOnline) return null

  return (
    <div
      role="status"
      aria-live="polite"
      style={{ position: 'sticky', top: 0, zIndex: 1100, width: '100%' }}
    >
      <Alert
        banner
        type="warning"
        showIcon
        icon={<DisconnectOutlined />}
        message={t('network.offlineBanner')}
      />
    </div>
  )
}
