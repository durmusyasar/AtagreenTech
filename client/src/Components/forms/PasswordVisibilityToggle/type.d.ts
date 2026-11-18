export interface PasswordVisibilityToggleProps {
  visible: boolean
  onToggle: () => void
  showLabel: string
  hideLabel: string
  /** Alan disabled iken toggle’ı da kilitle. */
  disabled?: boolean
  className?: string
}
