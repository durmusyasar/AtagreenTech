import { App } from 'antd'
import { useEffect } from 'react'
import { registerMessageInstance } from '../core/api/notifier'

/** API hatalarının bildirimleri antd `App` bağlamındaki message örneğiyle gösterilsin diye onu kaydeder. */
export function NotificationBridge() {
  const { message } = App.useApp()

  useEffect(() => {
    registerMessageInstance(message)
    return () => registerMessageInstance(null)
  }, [message])

  return null
}
