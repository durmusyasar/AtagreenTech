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
        serviceName: 'StockService',
        translationKey: 'services.stock',
        items: [
          {
            code: 'stock.root',
            route: '/stock',
            icon: 'warehouse',
            translationKey: 'menu.products',
            children: [
              {
                code: 'stock.units',
                route: '/units-of-measure',
                icon: 'units',
                translationKey: 'menu.unitsOfMeasure',
              },
            ],
          },
        ],
      },
    ]

    useNavigationStore.getState().setMenus(groups)

    const { flatItems } = useNavigationStore.getState()
    expect(flatItems.map((i) => i.code)).toEqual(['stock.root', 'stock.units'])
    expect(flatItems[1]?.serviceTranslationKey).toBe('services.stock')
  })

  it('clears menus on logout path', () => {
    useNavigationStore.getState().setMenus([
      {
        serviceName: 'General',
        translationKey: 'services.general',
        items: [{ code: 'general.dashboard', route: '/', icon: 'dashboard', translationKey: 'menu.dashboard' }],
      },
    ])

    useNavigationStore.getState().clear()
    expect(useNavigationStore.getState().groups).toEqual([])
    expect(useNavigationStore.getState().flatItems).toEqual([])
  })
})
