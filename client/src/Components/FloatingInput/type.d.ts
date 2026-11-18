import type { InputHTMLAttributes, ReactNode } from 'react'

export interface FloatingInputProps extends Omit<InputHTMLAttributes<HTMLInputElement>, 'size'> {
  /** Kayan etiket metni. */
  label: string
  /** Alan altı hata mesajı (çevrilmiş metin). */
  error?: string
  /** Sağ uç süsleme (ör. şifre göster/gizle). */
  endAdornment?: ReactNode
  /**
   * true iken dolu alanda sağda × butonu gösterilir; tıklanınca değer temizlenir.
   * Kontrollü kullanımda `onChange` ile boş string iletilir.
   */
  autoClear?: boolean
  /** × butonu erişilebilirlik etiketi. */
  clearLabel?: string
}
