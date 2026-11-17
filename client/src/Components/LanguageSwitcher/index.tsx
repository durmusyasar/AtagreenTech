import { useLanguage } from '@/core/context/useLanguage'
import type { Language } from '@/core/i18n/languages'
import type { LanguageSwitcherProps } from '@/Components/LanguageSwitcher/type'

/**
 * TR/EN dil seçici — cam efektli segmented kapsül.
 * Tercih `omnisync_lang` + sessionStore üzerinden kalıcıdır.
 */
export function LanguageSwitcher({ className = '' }: LanguageSwitcherProps) {
  const { language, setLanguage } = useLanguage()
  const current: Language = language === 'en' ? 'en' : 'tr'

  return (
    <div
      role="group"
      aria-label="Language selector"
      className={`inline-flex h-9 items-center gap-1 rounded-xl border border-subtle bg-elevated/70 p-1 shadow-sm backdrop-blur-md ${className}`.trim()}
    >
      <span className="pl-1.5 pr-0.5 text-muted" aria-hidden>
        <svg className="h-3.5 w-3.5" fill="none" viewBox="0 0 24 24" stroke="currentColor" strokeWidth={1.8}>
          <path
            strokeLinecap="round"
            strokeLinejoin="round"
            d="M21 12a9 9 0 01-9 9m9-9a9 9 0 00-9-9m9 9H3m9 9a9 9 0 01-9-9m9 9c1.657 0 3-4.03 3-9s-1.343-9-3-9m0 18c-1.657 0-3-4.03-3-9s1.343-9 3-9m-9 9a9 9 0 019-9"
          />
        </svg>
      </span>

      {(['tr', 'en'] as const).map((lang) => {
        const active = current === lang
        return (
          <button
            key={lang}
            type="button"
            onClick={() => {
              if (lang !== current) setLanguage(lang)
            }}
            className={`rounded-lg px-2.5 py-1 text-xs font-semibold uppercase tracking-wide transition-all duration-200 ${
              active
                ? 'bg-surface text-brand shadow-sm ring-1 ring-subtle'
                : 'text-secondary hover:text-primary'
            }`}
          >
            {lang}
          </button>
        )
      })}
    </div>
  )
}
