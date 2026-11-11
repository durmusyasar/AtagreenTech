import { useCallback, useEffect, useState, type DependencyList } from 'react'
import { ApiError } from './apiError'

export interface ApiQueryResult<T> {
  data: T | undefined
  error: ApiError | undefined
  loading: boolean
  reload: () => void
}

/**
 * `deps` değiştikçe `fetcher`'ı çalıştırır; bileşen kapanırsa veya bağımlılık değişirse bekleyen istek iptal edilir.
 * Hata bildirimi Axios interceptor'ında yapıldığı için burada yalnızca durum tutulur.
 */
export function useApiQuery<T>(fetcher: (signal: AbortSignal) => Promise<T>, deps: DependencyList): ApiQueryResult<T> {
  const [data, setData] = useState<T>()
  const [error, setError] = useState<ApiError>()
  const [loading, setLoading] = useState(true)
  const [reloadCount, setReloadCount] = useState(0)

  useEffect(() => {
    const controller = new AbortController()
    setLoading(true)

    fetcher(controller.signal).then(
      (result) => {
        if (controller.signal.aborted) return
        setData(result)
        setError(undefined)
        setLoading(false)
      },
      (reason: unknown) => {
        if (controller.signal.aborted) return
        setError(reason instanceof ApiError ? reason : undefined)
        setLoading(false)
      },
    )

    return () => controller.abort()
    // fetcher her render'da yeniden oluşturulur; yeniden çalıştırmayı çağıranın verdiği bağımlılıklar belirler.
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [...deps, reloadCount])

  const reload = useCallback(() => setReloadCount((count) => count + 1), [])

  return { data, error, loading, reload }
}
