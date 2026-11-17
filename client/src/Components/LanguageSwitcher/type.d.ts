export interface LanguageOption {
  code: string
  shortLabel: string
  label: string
}

export interface LanguageSwitcherProps {
  className?: string
  /** @deprecated Dropdown tek boyuttadır; geriye dönük uyumluluk için yok sayılır. */
  size?: 'small' | 'middle'
}
