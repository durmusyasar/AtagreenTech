import { Controller, useFormContext, type FieldValues } from 'react-hook-form'
import { FloatingInput } from '@/Components/FloatingInput'
import { useLanguage } from '@/core/context/useLanguage'
import type { FormFloatingInputProps } from '@/Components/forms/FormFloatingInput/type'

/**
 * FormProvider altındaki FloatingInput köprüsü.
 * `name` → RHF field + Zod schema key; hata mesajı i18n anahtarıysa `t()` ile çevrilir.
 */
export function FormFloatingInput<TFieldValues extends FieldValues>({
  name,
  transformValue,
  clearLabel,
  ...inputProps
}: FormFloatingInputProps<TFieldValues>) {
  const { control } = useFormContext<TFieldValues>()
  const { t } = useLanguage()

  return (
    <Controller
      name={name}
      control={control}
      render={({ field, fieldState }) => (
        <FloatingInput
          {...inputProps}
          name={field.name}
          ref={field.ref}
          value={field.value ?? ''}
          onBlur={field.onBlur}
          onChange={(event) => {
            const next = event.target.value
            field.onChange(transformValue ? transformValue(next) : next)
          }}
          error={fieldState.error?.message ? t(fieldState.error.message) : undefined}
          clearLabel={clearLabel ?? t('common.clear')}
        />
      )}
    />
  )
}
