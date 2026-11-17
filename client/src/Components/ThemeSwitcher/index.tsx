import { MoonOutlined, SunOutlined } from '@ant-design/icons'
import { Button, Segmented } from 'antd'
import { useThemeStore, type ThemeMode } from '@/stores/themeStore'
import type { ThemeSwitcherProps } from '@/Components/ThemeSwitcher/type'

const MODE_OPTIONS: { label: string; value: ThemeMode }[] = [
  { label: 'Light', value: 'light' },
  { label: 'Dark', value: 'dark' },
  { label: 'System', value: 'system' },
]

/**
 * Tema seçici. `toggle` resolved light/dark arasında geçer; `segmented` light/dark/system seçer.
 * Tercih `omnisync-theme` (Zustand persist) + `<html class="dark">` üzerinden kalıcıdır.
 */
export function ThemeSwitcher({ variant = 'toggle', className = '' }: ThemeSwitcherProps) {
  const { theme, resolvedTheme, setTheme, toggleTheme } = useThemeStore()

  if (variant === 'toggle') {
    const toLight = resolvedTheme === 'dark'
    return (
      <Button
        type="text"
        className={className}
        onClick={toggleTheme}
        aria-label={toLight ? 'Switch to light mode' : 'Switch to dark mode'}
        title={toLight ? 'Açık Tema' : 'Koyu Tema'}
        icon={toLight ? <SunOutlined style={{ color: '#fbbf24' }} /> : <MoonOutlined />}
      />
    )
  }

  return (
    <Segmented<ThemeMode>
      className={className}
      size="small"
      value={theme}
      aria-label="Theme mode selector"
      options={MODE_OPTIONS}
      onChange={(mode) => {
        if (mode !== theme) setTheme(mode)
      }}
    />
  )
}
