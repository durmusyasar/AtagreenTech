import type { ReactNode } from 'react'

export interface PublicAuthLayoutProps {
  children: ReactNode
  /** Kök shell’e ek Tailwind sınıfları. */
  className?: string
  /** `<main>` içerik alanına ek Tailwind sınıfları. */
  mainClassName?: string
}
