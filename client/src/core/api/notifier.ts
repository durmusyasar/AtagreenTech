import { message, type App } from 'antd'

type MessageInstance = ReturnType<typeof App.useApp>['message']

let instance: MessageInstance | null = null

/**
 * Interceptor'lar React dışında çalıştığı için antd'nin bağlamlı (tema ve dil uyumlu) message örneği burada tutulur;
 * `NotificationBridge` uygulama açılırken kaydeder.
 */
export function registerMessageInstance(next: MessageInstance | null): void {
  instance = next
}

/** Aynı `key` ile gelen bildirimler (ör. paralel isteklerin 401'leri) tek mesajda birleşir. */
export function notify(type: 'error' | 'warning', content: string, key?: string): void {
  ;(instance ?? message).open({ type, content, key, duration: 5 })
}
