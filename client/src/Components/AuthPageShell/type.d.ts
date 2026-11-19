import type { ReactNode } from 'react'

export type AuthPageBannerTone = 'error' | 'success' | 'info' | 'warning'

export interface AuthPageBanner {
  tone: AuthPageBannerTone
  message: ReactNode
}

export type AuthPageShellMaxWidth = 'sm' | 'md'

export interface AuthPageShellProps {
  title: string
  /** Kısa destek metni; link / vurgu için ReactNode olabilir. */
  description?: ReactNode
  /** Başlık altında, içerikten önce (form hataları, bilgilendirme). */
  banner?: AuthPageBanner
  /** Kart altı ikincil aksiyon (ör. girişe dön). */
  footer?: ReactNode
  /** Varsayılan `sm` (440px). */
  maxWidth?: AuthPageShellMaxWidth
  className?: string
  panelClassName?: string
  /** Token yokken yalnızca banner + footer gösterilebilir. */
  children?: ReactNode
}
