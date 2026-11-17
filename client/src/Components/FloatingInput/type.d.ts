import type { InputHTMLAttributes, ReactNode } from 'react'

export interface FloatingInputProps extends Omit<InputHTMLAttributes<HTMLInputElement>, 'size'> {
  /** Kayan etiket metni. */
  label: string
  /** Alan altı hata mesajı (çevrilmiş metin). */
  error?: string
  /** Sağ uç süsleme (ör. şifre göster/gizle). */
  endAdornment?: ReactNode
}
