import type { FieldPath, FieldValues } from 'react-hook-form'
import type { FloatingInputProps } from '@/Components/FloatingInput/type'

export interface FormFloatingInputProps<TFieldValues extends FieldValues>
  extends Omit<FloatingInputProps, 'name' | 'value' | 'onChange' | 'onBlur' | 'error'> {
  /** Zod / RHF alan adı — schema key ile birebir aynı olmalı. */
  name: FieldPath<TFieldValues>
  /** Değeri yazmadan önce dönüştür (ör. recovery kodunu upper-case). */
  transformValue?: (value: string) => string
}
