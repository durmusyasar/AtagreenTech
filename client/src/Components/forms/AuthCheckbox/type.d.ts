export interface AuthCheckboxProps {
  id?: string
  checked: boolean
  onChange: (checked: boolean) => void
  onBlur?: () => void
  label: string
  disabled?: boolean
  className?: string
}
