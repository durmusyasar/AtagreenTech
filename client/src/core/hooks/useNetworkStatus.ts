import { useEffect, useState } from 'react'

/**
 * Tarayıcının `online` / `offline` olaylarından anlık ağ durumunu izler.
 * Sunucu erişilebilirliğini ölçmez; yalnızca istemcinin ağ arayüzü bilgisidir.
 */
export function useNetworkStatus(): { isOnline: boolean } {
  const [isOnline, setIsOnline] = useState(() =>
    typeof navigator === 'undefined' ? true : navigator.onLine,
  )

  useEffect(() => {
    const handleOnline = () => setIsOnline(true)
    const handleOffline = () => setIsOnline(false)

    window.addEventListener('online', handleOnline)
    window.addEventListener('offline', handleOffline)
    return () => {
      window.removeEventListener('online', handleOnline)
      window.removeEventListener('offline', handleOffline)
    }
  }, [])

  return { isOnline }
}
