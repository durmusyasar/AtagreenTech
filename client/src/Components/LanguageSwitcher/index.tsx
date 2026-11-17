import { Tooltip } from 'antd'
import { useEffect, useId, useRef, useState } from 'react'
import { useLanguage } from '@/core/context/useLanguage'
import {
  isSupportedLanguage,
  LANGUAGE_LABELS,
  LANGUAGE_SHORT_LABELS,
  SUPPORTED_LANGUAGES,
  type Language,
} from '@/core/i18n/languages'
import type { LanguageOption, LanguageSwitcherProps } from '@/Components/LanguageSwitcher/type'

/** Yeni dil: `SUPPORTED_LANGUAGES` + etiket haritalarına ekle; UI otomatik büyür. */
const LANGUAGE_OPTIONS: LanguageOption[] = SUPPORTED_LANGUAGES.map((code) => ({
  code,
  shortLabel: LANGUAGE_SHORT_LABELS[code],
  label: LANGUAGE_LABELS[code],
}))

/**
 * Modern dil dropdown’ı — kompakt TR/EN satırları, tam ad Tooltip ile.
 * Tercih `omnisync_lang` + sessionStore üzerinden kalıcıdır.
 */
export function LanguageSwitcher({ className = '' }: LanguageSwitcherProps) {
  const { language, setLanguage } = useLanguage()
  const [open, setOpen] = useState(false)
  const containerRef = useRef<HTMLDivElement>(null)
  const listboxId = useId()

  const currentLang =
    LANGUAGE_OPTIONS.find((option) => option.code === language) ?? LANGUAGE_OPTIONS[0]

  useEffect(() => {
    if (!open) return

    const handlePointerDown = (event: MouseEvent) => {
      if (containerRef.current && !containerRef.current.contains(event.target as Node)) {
        setOpen(false)
      }
    }

    const handleKeyDown = (event: KeyboardEvent) => {
      if (event.key === 'Escape') setOpen(false)
    }

    document.addEventListener('mousedown', handlePointerDown)
    document.addEventListener('keydown', handleKeyDown)
    return () => {
      document.removeEventListener('mousedown', handlePointerDown)
      document.removeEventListener('keydown', handleKeyDown)
    }
  }, [open])

  const handleSelect = (code: string) => {
    if (!isSupportedLanguage(code)) return
    if (code !== language) setLanguage(code as Language)
    setOpen(false)
  }

  return (
    <div ref={containerRef} className={`relative inline-flex ${className}`.trim()}>
      <button
        type="button"
        onClick={() => setOpen((prev) => !prev)}
        aria-haspopup="listbox"
        aria-expanded={open}
        aria-controls={listboxId}
        aria-label="Select language"
        className={[
          'appearance-none inline-flex h-9 items-center gap-1.5 rounded-full border bg-surface px-2.5 text-xs font-medium text-primary',
          'shadow-sm transition-all duration-200 hover:bg-elevated focus:outline-none focus-visible:ring-2 focus-visible:ring-brand/25',
          open ? 'border-subtle bg-elevated' : 'border-subtle/80',
        ].join(' ')}
      >
        <svg
          className="h-3.5 w-3.5 text-muted"
          fill="none"
          viewBox="0 0 24 24"
          stroke="currentColor"
          strokeWidth={1.75}
          aria-hidden
        >
          <path
            strokeLinecap="round"
            strokeLinejoin="round"
            d="M21 12a9 9 0 01-9 9m9-9a9 9 0 00-9-9m9 9H3m9 9a9 9 0 01-9-9m9 9c1.657 0 3-4.03 3-9s-1.343-9-3-9m0 18c-1.657 0-3-4.03-3-9s1.343-9 3-9m-9 9a9 9 0 019-9"
          />
        </svg>
        <span className="min-w-[1.25rem] text-center font-semibold tracking-wider text-primary">
          {currentLang.shortLabel}
        </span>
        <svg
          className={`h-3 w-3 text-muted transition-transform duration-200 ${open ? 'rotate-180' : ''}`}
          fill="none"
          viewBox="0 0 24 24"
          stroke="currentColor"
          strokeWidth={2.25}
          aria-hidden
        >
          <path strokeLinecap="round" strokeLinejoin="round" d="M19 9l-7 7-7-7" />
        </svg>
      </button>

      {open ? (
        <div
          id={listboxId}
          role="listbox"
          className="absolute right-0 top-full z-50 mt-2.5 origin-top-right rounded-2xl border border-subtle/70 bg-surface/95 p-1 shadow-xl shadow-slate-900/[0.08] ring-1 ring-black/[0.03] backdrop-blur-xl dark:shadow-black/50 dark:ring-white/[0.06]"
        >
          <div className="flex flex-col gap-0.5">
            {LANGUAGE_OPTIONS.map((lang) => {
              const isSelected = lang.code === currentLang.code
              return (
                <Tooltip key={lang.code} title={lang.label} placement="left" mouseEnterDelay={0.12}>
                  <button
                    type="button"
                    role="option"
                    aria-selected={isSelected}
                    aria-label={lang.label}
                    onClick={() => handleSelect(lang.code)}
                    className={[
                      'appearance-none relative flex h-8 w-12 items-center justify-center rounded-xl border-0 bg-transparent text-[11px] font-semibold tracking-wider outline-none transition-all duration-150',
                      isSelected
                        ? 'bg-brand/12 text-brand'
                        : 'text-secondary hover:bg-elevated hover:text-primary',
                    ].join(' ')}
                  >
                    {lang.shortLabel}
                    {isSelected ? (
                      <span
                        className="absolute bottom-1 left-1/2 h-0.5 w-3 -translate-x-1/2 rounded-full bg-brand"
                        aria-hidden
                      />
                    ) : null}
                  </button>
                </Tooltip>
              )
            })}
          </div>
        </div>
      ) : null}
    </div>
  )
}
