import { Typography } from 'antd'
import type { CSSProperties } from 'react'
import { useTenantTheme } from '../core/context/useTenantTheme'

interface BrandMarkProps {
  /** Logo yüksekliği (px). */
  size?: number
  /** Marka adını metin olarak göster. */
  showName?: boolean
  /** Metin stili (ör. sider'da `colorPrimary`). */
  nameStyle?: CSSProperties
  className?: string
}

/** Tenant logosunu (ve isteğe bağlı adını) gösterir; config yoksa varsayılan favicon kullanılır. */
export function BrandMark({ size = 24, showName = true, nameStyle, className }: BrandMarkProps) {
  const { config } = useTenantTheme()
  const src = config.logoUrl || '/favicon.svg'

  return (
    <span className={className} style={{ display: 'inline-flex', alignItems: 'center', gap: 10, minWidth: 0 }}>
      <img src={src} alt="" width={size} height={size} style={{ objectFit: 'contain', flexShrink: 0 }} />
      {showName && (
        <Typography.Text strong ellipsis style={nameStyle}>
          {config.name}
        </Typography.Text>
      )}
    </span>
  )
}
