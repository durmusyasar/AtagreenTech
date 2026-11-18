import { queryOptions } from '@tanstack/react-query'
import { axiosClient } from '@/core/api/axiosClient'
import type { NavigationMenusResponse } from '@/Components/Sidebar/type'
import { queryKeys } from '@/services/queryKeys'

export const navigationService = {
  async getMenus(signal?: AbortSignal): Promise<NavigationMenusResponse> {
    const { data } = await axiosClient.get<NavigationMenusResponse>('/navigation/menus', { signal })
    return data
  },
}

export const navigationQueries = {
  menus: () =>
    queryOptions({
      queryKey: [...queryKeys.navigation, 'menus'],
      queryFn: ({ signal }) => navigationService.getMenus(signal),
      staleTime: 60_000,
    }),
}
