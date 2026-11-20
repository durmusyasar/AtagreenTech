import { beforeEach, describe, expect, it } from 'vitest'
import { useNavigationStore } from '@/stores/navigationStore'
import type { NavigationMenuGroup } from '@/Components/Sidebar/type'

describe('navigationStore', () => {
  beforeEach(() => {
    useNavigationStore.getState().clear()
  })

  it('flattens nested menu items for command palette', () => {
    const groups: NavigationMenuGroup[] = [
      {
        moduleCode: 'STOCK',
        serviceName: 'StockService',
        title: 'Stok',
        translations: { tr: 'Stok', en: 'Stock' },
        items: [
          {
            code: 'stock.root',
            route: '/stock',
            icon: 'warehouse',
            title: 'Ürünler',
            translations: { tr: 'Ürünler', en: 'Products' },
            children: [
              {
                code: 'stock.units',
                route: '/units-of-measure',
                icon: 'units',
                title: 'Ölçü Birimleri',
                translations: { tr: 'Ölçü Birimleri', en: 'Units of Measure' },
              },
            ],
          },
        ],
      },
    ]

    useNavigationStore.getState().setMenus(groups)

    const { flatItems } = useNavigationStore.getState()
    expect(flatItems.map((i) => i.code)).toEqual(['stock.root', 'stock.units'])
    expect(flatItems[1]?.moduleTitle).toBe('Stok')
    expect(flatItems[1]?.translations.en).toBe('Units of Measure')
  })

  it('clears menus on logout path', () => {
    useNavigationStore.getState().setMenus([
      {
        moduleCode: 'GENERAL',
        serviceName: 'General',
        title: 'Genel',
        translations: { tr: 'Genel', en: 'General' },
        items: [
          {
            code: 'general.dashboard',
            route: '/',
            icon: 'dashboard',
            title: 'Genel Bakış',
            translations: { tr: 'Genel Bakış', en: 'Overview' },
          },
        ],
      },
    ])

    useNavigationStore.getState().clear()
    expect(useNavigationStore.getState().groups).toEqual([])
    expect(useNavigationStore.getState().flatItems).toEqual([])
  })
})
