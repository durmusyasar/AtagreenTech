import type { FieldPath, FieldValues } from 'react-hook-form'
import type { AuthCheckboxProps } from '@/Components/forms/AuthCheckbox/type'

export interface FormAuthCheckboxProps<TFieldValues extends FieldValues>
  extends Omit<AuthCheckboxProps, 'checked' | 'onChange'> {
  name: FieldPath<TFieldValues>
}
