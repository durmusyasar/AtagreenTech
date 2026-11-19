export type BackToLoginLinkVariant = 'text' | 'button'

export interface BackToLoginLinkProps {
  /**
   * `text` — kart altı ikincil link (← Girişe dön).
   * `button` — tam genişlik birincil CTA (başarı ekranları).
   */
  variant?: BackToLoginLinkVariant
  /** Varsayılan: `auth.backToLogin`. */
  label?: string
  /** Örn. `invited=1` → `/login?invited=1`. */
  search?: string
  className?: string
}
