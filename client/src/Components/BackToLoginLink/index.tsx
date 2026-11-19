import { Link } from 'react-router-dom'
import { useLanguage } from '@/core/context/useLanguage'
import type { BackToLoginLinkProps } from '@/Components/BackToLoginLink/type'

const VARIANT_CLASS = {
  text: 'text-xs font-medium text-secondary transition-colors hover:text-primary',
  button:
    'block w-full rounded-xl bg-brand px-4 py-2.5 text-center text-sm font-medium text-white transition-colors hover:bg-brand-hover',
} as const

/** Public auth sayfalarından giriş ekranına dönüş linki / CTA. */
export function BackToLoginLink({
  variant = 'text',
  label,
  search,
  className = '',
}: BackToLoginLinkProps) {
  const { t } = useLanguage()
  const to = search ? `/login?${search.replace(/^\?/, '')}` : '/login'
  const text = label ?? t('auth.backToLogin')

  return (
    <Link to={to} className={`${VARIANT_CLASS[variant]} ${className}`.trim()}>
      {variant === 'text' ? `← ${text}` : text}
    </Link>
  )
}
