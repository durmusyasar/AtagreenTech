import { create } from 'zustand'
import { persist } from 'zustand/middleware'

export type ThemeMode = 'light' | 'dark' | 'system'
export type ResolvedTheme = 'light' | 'dark'

interface ThemeState {
  theme: ThemeMode
  resolvedTheme: ResolvedTheme
  setTheme: (theme: ThemeMode) => void
  toggleTheme: () => void
}

export const THEME_STORAGE_KEY = 'omnisync-theme'

const applyThemeToDocument = (mode: ThemeMode): ResolvedTheme => {
  const systemPrefersDark =
    typeof window !== 'undefined' && window.matchMedia('(prefers-color-scheme: dark)').matches

  const resolved: ResolvedTheme = mode === 'system' ? (systemPrefersDark ? 'dark' : 'light') : mode

  if (typeof document !== 'undefined') {
    const root = document.documentElement
    if (resolved === 'dark') {
      root.classList.add('dark')
    } else {
      root.classList.remove('dark')
    }
  }

  return resolved
}

function initialResolvedTheme(): ResolvedTheme {
  if (typeof window === 'undefined') return 'light'
  return window.matchMedia('(prefers-color-scheme: dark)').matches ? 'dark' : 'light'
}

export const useThemeStore = create<ThemeState>()(
  persist(
    (set, get) => ({
      theme: 'system',
      resolvedTheme: initialResolvedTheme(),

      setTheme: (theme) => {
        const resolvedTheme = applyThemeToDocument(theme)
        set({ theme, resolvedTheme })
      },

      toggleTheme: () => {
        const current = get().resolvedTheme
        const next: ThemeMode = current === 'dark' ? 'light' : 'dark'
        const resolvedTheme = applyThemeToDocument(next)
        set({ theme: next, resolvedTheme })
      },
    }),
    {
      name: THEME_STORAGE_KEY,
      partialize: (state) => ({ theme: state.theme }),
      onRehydrateStorage: () => (state) => {
        if (!state) return
        const resolvedTheme = applyThemeToDocument(state.theme)
        useThemeStore.setState({ resolvedTheme })
      },
    },
  ),
)

/** Sistem teması değişince `system` modundaysa document + store güncellenir. */
if (typeof window !== 'undefined') {
  window.matchMedia('(prefers-color-scheme: dark)').addEventListener('change', () => {
    const { theme, setTheme } = useThemeStore.getState()
    if (theme === 'system') {
      setTheme('system')
    }
  })
}
