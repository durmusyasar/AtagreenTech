import { QueryClientProvider } from '@tanstack/react-query'
import { App as AntApp } from 'antd'
import { RouterProvider } from 'react-router-dom'
import { NotificationBridge } from '@/Components/NotificationBridge'
import { OfflineBanner } from '@/Components/OfflineBanner'
import { SessionHydrator } from '@/Components/SessionHydrator'
import { clearOnSessionChange, createQueryClient } from '@/core/api/queryClient'
import { TenantThemeProvider } from '@/core/context/TenantThemeProvider'
import { sessionStore } from '@/core/session'
import { appRouter } from '@/routes'

const queryClient = createQueryClient()
clearOnSessionChange(queryClient, sessionStore)

export function App() {
  return (
    <QueryClientProvider client={queryClient}>
      <TenantThemeProvider>
        <AntApp>
          <SessionHydrator>
            <NotificationBridge />
            <OfflineBanner />
            <RouterProvider router={appRouter} />
          </SessionHydrator>
        </AntApp>
      </TenantThemeProvider>
    </QueryClientProvider>
  )
}
