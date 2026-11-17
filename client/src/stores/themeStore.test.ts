/** @vitest-environment happy-dom */
import { beforeEach, describe, expect, it, vi } from 'vitest'

describe('useThemeStore', () => {
  beforeEach(() => {
    vi.resetModules()
    localStorage.clear()
    document.documentElement.classList.remove('dark')
  })

  it('applies dark class when theme is dark', async () => {
    const { useThemeStore } = await import('./themeStore')
    useThemeStore.getState().setTheme('dark')
    expect(useThemeStore.getState().resolvedTheme).toBe('dark')
    expect(document.documentElement.classList.contains('dark')).toBe(true)
  })

  it('removes dark class when theme is light', async () => {
    const { useThemeStore } = await import('./themeStore')
    useThemeStore.getState().setTheme('dark')
    useThemeStore.getState().setTheme('light')
    expect(useThemeStore.getState().resolvedTheme).toBe('light')
    expect(document.documentElement.classList.contains('dark')).toBe(false)
  })

  it('toggleTheme switches between light and dark modes', async () => {
    const { useThemeStore } = await import('./themeStore')
    useThemeStore.getState().setTheme('light')
    useThemeStore.getState().toggleTheme()
    expect(useThemeStore.getState().theme).toBe('dark')
    useThemeStore.getState().toggleTheme()
    expect(useThemeStore.getState().theme).toBe('light')
  })

  it('persists theme preference under omnisync-theme', async () => {
    const { useThemeStore, THEME_STORAGE_KEY } = await import('./themeStore')
    useThemeStore.getState().setTheme('dark')
    const raw = localStorage.getItem(THEME_STORAGE_KEY)
    expect(raw).toBeTruthy()
    expect(JSON.parse(raw!).state.theme).toBe('dark')
  })

  it('defaults to system (prefers-color-scheme) when nothing is persisted', async () => {
    const { useThemeStore } = await import('./themeStore')
    expect(useThemeStore.getState().theme).toBe('system')
  })
})
