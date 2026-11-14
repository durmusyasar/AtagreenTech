import { QueryClient, type QueryKey } from '@tanstack/react-query'
import type { SessionState } from '../session'
import { ApiError } from './apiError'

declare module '@tanstack/react-query' {
  interface Register {
    /** Axios interceptor'ı her hatayı `ApiError`'a çevirir. */
    defaultError: ApiError
  }
}

/** Verinin taze sayıldığı süre; bu sürede aynı sorguyu açan sayfalar önbellekten okur. */
const STALE_TIME_MS = 30_000

/** İstemci hataları (4xx) tekrar denemekle düzelmez; ağ ve sunucu hataları bir kez daha denenir. */
export function shouldRetry(failureCount: number, error: unknown): boolean {
  const status = error instanceof ApiError ? error.status : null
  return failureCount < 1 && (status === null || status >= 500)
}

export function createQueryClient(): QueryClient {
  return new QueryClient({
    defaultOptions: {
      queries: { staleTime: STALE_TIME_MS, retry: shouldRetry },
      // Kayıt istekleri tekrarlanırsa aynı hareket iki kez işlenebilir.
      mutations: { retry: false },
    },
  })
}

type SessionSource = {
  getSnapshot: () => Pick<SessionState, 'token' | 'tenant'>
  subscribe: (listener: () => void) => () => void
}

/** Token veya tenant değişince önbellek boşaltılır; bir oturumun ya da tenant'ın verisi diğerinde görünmemeli. */
export function clearOnSessionChange(queryClient: QueryClient, session: SessionSource): () => void {
  let { token, tenant } = session.getSnapshot()

  return session.subscribe(() => {
    const next = session.getSnapshot()
    if (next.token !== token || next.tenant !== tenant) queryClient.clear()
    ;({ token, tenant } = next)
  })
}

/** Kayıttan sonra etkilenen alanların sorguları bayatlatılır; ekranda açık olanlar hemen yeniden yüklenir. */
export function invalidate(queryClient: QueryClient, ...keys: QueryKey[]): Promise<void> {
  return Promise.all(keys.map((queryKey) => queryClient.invalidateQueries({ queryKey }))).then(() => undefined)
}
