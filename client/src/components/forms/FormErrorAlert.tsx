import { Alert } from 'antd'

/** Bir alana bağlanamayan sunucu hatalarını formun üstünde gösterir. */
export function FormErrorAlert({ message, onClose }: { message: string | undefined; onClose: () => void }) {
  if (!message) return null
  return <Alert type="error" showIcon closable title={message} onClose={onClose} style={{ marginBottom: 16 }} />
}
