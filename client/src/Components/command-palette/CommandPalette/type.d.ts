import type { ReactNode } from 'react'
import type { MessageKey } from '@/core/i18n/messages'

export interface PaletteCommand {
  id: string
  /** Önceden çözülmüş başlık (API menü veya `t(key)`). */
  title: string
  categoryKey: MessageKey
  /** Varsa `categoryKey` çevirisi yerine kullanılır (modül adı, #65/#69). */
  categoryLabelOverride?: string
  keywords: string
  icon: ReactNode
  run: () => void
}

export interface CommandPaletteProps {
  open: boolean
  onOpenChange: (open: boolean) => void
}
