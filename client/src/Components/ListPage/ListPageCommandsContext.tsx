import { createContext, useContext } from 'react'
import type { ListPageCommandsContextValue } from '@/Components/ListPage/type'

const ListPageCommandsContext = createContext<ListPageCommandsContextValue | null>(null)

export const ListPageCommandsProvider = ListPageCommandsContext.Provider

/** ConnectedListPage filtre alanlarından ara/temizle/create tetiklemek için. */
export function useListPageCommands(): ListPageCommandsContextValue {
  const value = useContext(ListPageCommandsContext)
  if (!value) {
    throw new Error('useListPageCommands must be used within ConnectedListPage')
  }
  return value
}
