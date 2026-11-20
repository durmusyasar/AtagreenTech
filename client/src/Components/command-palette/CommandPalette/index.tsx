import { BgColorsOutlined, GlobalOutlined, LogoutOutlined, PlusOutlined } from '@ant-design/icons'
import { Empty, Input, Modal, type InputRef } from 'antd'
import {
  useCallback,
  useEffect,
  useMemo,
  useRef,
  useState,
  type KeyboardEvent as ReactKeyboardEvent,
} from 'react'
import { useNavigate } from 'react-router-dom'
import { useAuth } from '@/core/context/useAuth'
import { useLanguage } from '@/core/context/useLanguage'
import { usePermissions } from '@/core/hooks/usePermissions'
import { useNavigationMenus } from '@/hooks/useNavigationMenus'
import { useNavigationStore } from '@/stores/navigationStore'
import { useThemeStore } from '@/stores/themeStore'
import { resolveMenuIcon } from '@/utils/iconMapper'
import { resolveNavTitle } from '@/utils/resolveNavTitle'
import { filterByQuery, isCommandPaletteToggle } from '@/Components/command-palette/commandPaletteSearch'
import type { CommandPaletteProps, PaletteCommand } from '@/Components/command-palette/CommandPalette/type'

const NAV_KEYWORDS: Record<string, string> = {
  '/': 'home overview genel bakış dashboard',
  '/units-of-measure': 'uom unit ölçü birim',
  '/categories': 'category kategori',
  '/products': 'product ürün sku',
  '/warehouses': 'warehouse depo',
  '/stock-movements': 'movement hareket transfer stock',
  '/stock-balances': 'balance bakiye inventory',
  '/account/security': 'security güvenlik mfa password şifre session oturum',
  '/admin/users': 'users kullanıcı admin invite davet',
  '/admin/roles': 'roles permissions rbac rol izin',
  '/admin/system': 'system sistem health bağlantı token tenant diagnostics',
  '/system/menus': 'menu menü navigation katalog',
  '/system/tenants': 'tenant kiracı module abonelik subscription',
}

/**
 * Ctrl/Cmd+K komut paleti (#69): brand kontrast, custom scrollbar, scrollIntoView.
 * Sayfalar `flatItems` (abonelik + izin filtreli menü) üzerinden gelir.
 */
export function CommandPalette({ open, onOpenChange }: CommandPaletteProps) {
  const navigate = useNavigate()
  const { language, setLanguage, t } = useLanguage()
  const { signOut } = useAuth()
  const toggleTheme = useThemeStore((state) => state.toggleTheme)
  const access = usePermissions()
  useNavigationMenus()
  const flatItems = useNavigationStore((s) => s.flatItems)
  const [query, setQuery] = useState('')
  const [activeIndex, setActiveIndex] = useState(0)
  const inputRef = useRef<InputRef>(null)
  const itemRefs = useRef<(HTMLButtonElement | null)[]>([])

  const setOpen = useCallback(
    (next: boolean) => {
      if (!next) {
        setQuery('')
        setActiveIndex(0)
      }
      onOpenChange(next)
    },
    [onOpenChange],
  )

  const go = useCallback(
    (path: string) => {
      navigate(path)
      setOpen(false)
    },
    [navigate, setOpen],
  )

  const commands = useMemo<PaletteCommand[]>(() => {
    // Backend zaten tenant aboneliği + izin filtreledi; ⌘K yalnızca yetkili ekranları listeler.
    const pageCommands = flatItems.map((item): PaletteCommand => {
      const moduleLabel = resolveNavTitle(item.moduleTranslations, item.moduleTitle, language)
      return {
        id: item.code,
        title: resolveNavTitle(item.translations, item.title, language),
        categoryKey: 'command.category.pages',
        categoryLabelOverride: moduleLabel,
        keywords: `${NAV_KEYWORDS[item.route] ?? ''} ${item.serviceName} ${moduleLabel}`,
        icon: resolveMenuIcon(item.icon),
        run: () => go(item.route),
      }
    })

    const actionCommands: PaletteCommand[] = []
    if (access.can('stock.write')) {
      actionCommands.push({
        id: 'new-product',
        title: t('command.newProduct'),
        categoryKey: 'command.category.actions',
        keywords: 'new create ekle ürün product',
        icon: <PlusOutlined />,
        run: () => go('/products?action=new'),
      })
    }

    return [
      ...pageCommands,
      ...actionCommands,
      {
        id: 'theme-toggle',
        title: t('command.toggleTheme'),
        categoryKey: 'command.category.system',
        keywords: 'theme tema dark light dark mode',
        icon: <BgColorsOutlined />,
        run: () => {
          toggleTheme()
          setOpen(false)
        },
      },
      {
        id: 'lang-toggle',
        title: t('command.toggleLanguage'),
        categoryKey: 'command.category.system',
        keywords: 'language dil tr en english türkçe',
        icon: <GlobalOutlined />,
        run: () => {
          setLanguage(language === 'tr' ? 'en' : 'tr')
          setOpen(false)
        },
      },
      {
        id: 'sign-out',
        title: t('header.signOut'),
        categoryKey: 'command.category.system',
        keywords: 'logout çıkış sign out exit',
        icon: <LogoutOutlined />,
        run: () => {
          setOpen(false)
          void signOut()
        },
      },
    ]
  }, [access, flatItems, go, language, setLanguage, setOpen, signOut, t, toggleTheme])

  const searchable = useMemo(
    () =>
      commands.map((command) => {
        const categoryLabel = command.categoryLabelOverride ?? t(command.categoryKey)
        return {
          ...command,
          categoryLabel,
          searchText: `${command.title} ${categoryLabel} ${command.keywords}`,
        }
      }),
    [commands, t],
  )

  const filtered = useMemo(() => filterByQuery(searchable, query), [searchable, query])
  const safeIndex = filtered.length === 0 ? 0 : Math.min(activeIndex, filtered.length - 1)

  useEffect(() => {
    const onKeyDown = (event: KeyboardEvent) => {
      if (!isCommandPaletteToggle(event)) return
      event.preventDefault()
      setOpen(!open)
    }
    window.addEventListener('keydown', onKeyDown)
    return () => window.removeEventListener('keydown', onKeyDown)
  }, [open, setOpen])

  useEffect(() => {
    if (!open) return
    const timer = window.setTimeout(() => inputRef.current?.focus(), 0)
    return () => window.clearTimeout(timer)
  }, [open])

  useEffect(() => {
    itemRefs.current[safeIndex]?.scrollIntoView({ block: 'nearest' })
  }, [safeIndex, filtered.length])

  const onInputKeyDown = (event: ReactKeyboardEvent<HTMLInputElement>) => {
    if (event.key === 'ArrowDown') {
      event.preventDefault()
      setActiveIndex(filtered.length === 0 ? 0 : (safeIndex + 1) % filtered.length)
      return
    }
    if (event.key === 'ArrowUp') {
      event.preventDefault()
      setActiveIndex(filtered.length === 0 ? 0 : (safeIndex - 1 + filtered.length) % filtered.length)
      return
    }
    if (event.key === 'Enter') {
      event.preventDefault()
      filtered[safeIndex]?.run()
    }
  }

  return (
    <Modal
      open={open}
      onCancel={() => setOpen(false)}
      footer={null}
      closable={false}
      width={560}
      destroyOnHidden
      className="command-palette-modal"
      styles={{ body: { padding: 0 } }}
      style={{ top: 80 }}
    >
      <div className="border-b border-subtle px-4 py-3">
        <Input
          ref={inputRef}
          size="large"
          variant="borderless"
          placeholder={t('command.placeholder')}
          value={query}
          onChange={(event) => {
            setQuery(event.target.value)
            setActiveIndex(0)
          }}
          onKeyDown={onInputKeyDown}
          aria-label={t('command.placeholder')}
          aria-activedescendant={
            filtered[safeIndex] ? `command-item-${filtered[safeIndex].id}` : undefined
          }
        />
      </div>

      <div
        className="custom-scrollbar max-h-80 space-y-1 overflow-y-auto p-2"
        role="listbox"
        aria-label={t('command.title')}
      >
        {filtered.length === 0 ? (
          <Empty
            image={Empty.PRESENTED_IMAGE_SIMPLE}
            description={t('command.empty')}
            className="py-8"
          />
        ) : (
          filtered.map((item, index) => {
            const active = index === safeIndex
            return (
              <button
                key={item.id}
                id={`command-item-${item.id}`}
                ref={(el) => {
                  itemRefs.current[index] = el
                }}
                type="button"
                role="option"
                aria-selected={active}
                className={[
                  'command-palette-item flex w-full appearance-none items-center justify-between gap-3 rounded-xl border px-3.5 py-2.5 text-left transition-colors',
                  'outline-none',
                  active
                    ? 'border-brand/30 bg-brand/15 text-primary'
                    : 'border-transparent text-primary hover:bg-elevated',
                ].join(' ')}
                onMouseEnter={() => setActiveIndex(index)}
                onClick={item.run}
              >
                <div className="flex min-w-0 items-center gap-3">
                  <span
                    className={[
                      'inline-flex h-8 w-8 shrink-0 items-center justify-center rounded-lg text-base leading-none',
                      active ? 'bg-brand/20 text-brand' : 'bg-elevated text-secondary',
                    ].join(' ')}
                  >
                    {item.icon}
                  </span>
                  <div className="min-w-0 truncate">
                    <div className="truncate text-sm font-medium text-primary">{item.title}</div>
                    <div className="truncate text-xs text-secondary">{item.categoryLabel}</div>
                  </div>
                </div>
                <span
                  className={[
                    'ml-3 shrink-0 text-xs font-medium',
                    active ? 'text-brand' : 'text-muted',
                  ].join(' ')}
                >
                  {t('command.go')}
                </span>
              </button>
            )
          })
        )}
      </div>

      <div className="flex items-center justify-between border-t border-subtle bg-elevated/60 px-4 py-2.5 text-[11px] text-muted">
        <div className="flex items-center gap-3">
          <span>↑↓</span>
          <span>↵ {t('command.go')}</span>
        </div>
        <span>⌘K / Ctrl+K</span>
      </div>
    </Modal>
  )
}
