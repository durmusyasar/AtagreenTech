import { Controller, useFormContext, type FieldValues } from 'react-hook-form'
import { AuthCheckbox } from '@/Components/forms/AuthCheckbox'
import type { FormAuthCheckboxProps } from '@/Components/forms/FormAuthCheckbox/type'

/** FormProvider altındaki AuthCheckbox köprüsü (`name` ↔ boolean alan). */
export function FormAuthCheckbox<TFieldValues extends FieldValues>({
  name,
  ...checkboxProps
}: FormAuthCheckboxProps<TFieldValues>) {
  const { control } = useFormContext<TFieldValues>()

  return (
    <Controller
      name={name}
      control={control}
      render={({ field }) => (
        <AuthCheckbox
          {...checkboxProps}
          checked={Boolean(field.value)}
          onChange={field.onChange}
        />
      )}
    />
  )
}
