import { useThemeStore, type ThemeMode } from '@/stores/themeStore'
import type { ThemeSwitcherProps } from '@/Components/ThemeSwitcher/type'

const MODE_OPTIONS: { label: string; value: ThemeMode }[] = [
  { label: 'Light', value: 'light' },
  { label: 'Dark', value: 'dark' },
  { label: 'System', value: 'system' },
]

/**
 * Tema seçici. Varsayılan `toggle`: cam efektli pill ikon.
 * `segmented`: light / dark / system (ayar panelleri için).
 */
export function ThemeSwitcher({ variant = 'toggle', className = '' }: ThemeSwitcherProps) {
  const { theme, resolvedTheme, setTheme, toggleTheme } = useThemeStore()
  const isDark = resolvedTheme === 'dark'

  if (variant === 'segmented') {
    return (
      <div
        role="group"
        aria-label="Theme mode selector"
        className={`inline-flex h-9 items-center gap-1 rounded-xl border border-subtle bg-elevated/70 p-1 shadow-sm backdrop-blur-md ${className}`.trim()}
      >
        {MODE_OPTIONS.map((option) => {
          const active = theme === option.value
          return (
            <button
              key={option.value}
              type="button"
              onClick={() => {
                if (option.value !== theme) setTheme(option.value)
              }}
              className={`rounded-lg px-2.5 py-1 text-xs font-semibold transition-all duration-200 ${
                active
                  ? 'bg-surface text-brand shadow-sm ring-1 ring-subtle'
                  : 'text-secondary hover:text-primary'
              }`}
            >
              {option.label}
            </button>
          )
        })}
      </div>
    )
  }

  return (
    <button
      type="button"
      onClick={toggleTheme}
      aria-label={isDark ? 'Switch to light theme' : 'Switch to dark theme'}
      title={isDark ? 'Açık Tema' : 'Koyu Tema'}
      className={`group relative inline-flex h-9 w-9 items-center justify-center rounded-xl border border-subtle bg-elevated/70 text-secondary shadow-sm backdrop-blur-md transition-all duration-200 hover:bg-elevated hover:text-primary focus:outline-none focus:ring-2 focus:ring-brand/30 ${className}`.trim()}
    >
      {isDark ? (
        <svg
          className="h-4 w-4 text-amber-400 transition-transform duration-300 group-hover:rotate-45"
          fill="none"
          viewBox="0 0 24 24"
          stroke="currentColor"
          strokeWidth={2}
          aria-hidden
        >
          <path
            strokeLinecap="round"
            strokeLinejoin="round"
            d="M12 3v1m0 16v1m9-9h-1M4 12H3m15.364 6.364l-.707-.707M6.343 6.343l-.707-.707m12.728 0l-.707.707M6.343 17.657l-.707.707M16 12a4 4 0 11-8 0 4 4 0 018 0z"
          />
        </svg>
      ) : (
        <svg
          className="h-4 w-4 text-slate-700 transition-transform duration-300 group-hover:-rotate-12 dark:text-slate-200"
          fill="none"
          viewBox="0 0 24 24"
          stroke="currentColor"
          strokeWidth={2}
          aria-hidden
        >
          <path
            strokeLinecap="round"
            strokeLinejoin="round"
            d="M20.354 15.354A9 9 0 018.646 3.646 9.003 9.003 0 0012 21a9.003 9.003 0 008.354-5.646z"
          />
        </svg>
      )}
    </button>
  )
}
