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
 * Ölçeklenebilir dil dropdown’ı (tek buton + listbox).
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
    <div ref={containerRef} className={`relative inline-block ${className}`.trim()}>
      <button
        type="button"
        onClick={() => setOpen((prev) => !prev)}
        aria-haspopup="listbox"
        aria-expanded={open}
        aria-controls={listboxId}
        aria-label="Select language"
        className="appearance-none inline-flex h-9 items-center gap-2 rounded-xl border border-subtle bg-surface px-3 text-xs font-medium text-primary shadow-sm transition-colors duration-150 hover:bg-elevated focus:outline-none focus:ring-2 focus:ring-brand/30"
      >
        <svg
          className="h-4 w-4 text-secondary"
          fill="none"
          viewBox="0 0 24 24"
          stroke="currentColor"
          strokeWidth={1.8}
          aria-hidden
        >
          <path
            strokeLinecap="round"
            strokeLinejoin="round"
            d="M21 12a9 9 0 01-9 9m9-9a9 9 0 00-9-9m9 9H3m9 9a9 9 0 01-9-9m9 9c1.657 0 3-4.03 3-9s-1.343-9-3-9m0 18c-1.657 0-3-4.03-3-9s1.343-9 3-9m-9 9a9 9 0 019-9"
          />
        </svg>
        <span className="font-semibold tracking-wide">{currentLang.shortLabel}</span>
        <svg
          className={`h-3.5 w-3.5 text-muted transition-transform duration-150 ${open ? 'rotate-180' : ''}`}
          fill="none"
          viewBox="0 0 24 24"
          stroke="currentColor"
          strokeWidth={2}
          aria-hidden
        >
          <path strokeLinecap="round" strokeLinejoin="round" d="M19 9l-7 7-7-7" />
        </svg>
      </button>

      {open ? (
        <div
          id={listboxId}
          role="listbox"
          className="absolute right-0 z-50 mt-2 w-40 origin-top-right rounded-xl border border-subtle bg-surface p-1.5 shadow-lg shadow-slate-900/10 dark:shadow-black/40"
        >
          {LANGUAGE_OPTIONS.map((lang) => {
            const isSelected = lang.code === currentLang.code
            return (
              <button
                key={lang.code}
                type="button"
                role="option"
                aria-selected={isSelected}
                onClick={() => handleSelect(lang.code)}
                className={`appearance-none flex w-full items-center justify-between rounded-lg px-3 py-2 text-xs transition-colors ${
                  isSelected
                    ? 'bg-brand/10 font-semibold text-brand'
                    : 'text-secondary hover:bg-elevated hover:text-primary'
                }`}
              >
                <span>{lang.label}</span>
                <span className="font-mono text-[11px] opacity-75">{lang.shortLabel}</span>
              </button>
            )
          })}
        </div>
      ) : null}
    </div>
  )
}
