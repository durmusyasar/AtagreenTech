import type { CSSProperties } from 'react'

export interface BrandMarkProps {
  /** Logo yüksekliği (px). */
  size?: number
  /** Marka adını metin olarak göster. */
  showName?: boolean
  /** Metin stili (ör. sider'da `colorPrimary`). */
  nameStyle?: CSSProperties
  className?: string
}
