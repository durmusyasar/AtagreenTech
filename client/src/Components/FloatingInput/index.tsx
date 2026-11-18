import { useId } from 'react'
import type { FloatingInputProps } from '@/Components/FloatingInput/type'

/**
 * Material outlined floating label.
 * Boşken etiket alanda ortada; odak / doluyken üst border’ın üzerine oturur (kesik outline).
 */
export function FloatingInput({
  id,
  label,
  error,
  endAdornment,
  className = '',
  disabled,
  value,
  ...inputProps
}: FloatingInputProps) {
  const generatedId = useId()
  const inputId = id ?? generatedId
  const errorId = error ? `${inputId}-error` : undefined
  const hasEnd = Boolean(endAdornment)

  return (
    <div className={`w-full min-w-0 max-w-full ${className}`.trim()}>
      <div className="relative w-full min-w-0">
        <input
          {...inputProps}
          id={inputId}
          value={value}
          disabled={disabled}
          placeholder=" "
          aria-invalid={Boolean(error)}
          aria-describedby={errorId}
          className={[
            'peer box-border appearance-none w-full max-w-full min-w-0 rounded-xl border bg-transparent',
            'h-12 text-sm text-primary outline-none transition-[border-color,box-shadow] duration-150',
            hasEnd ? 'pl-3.5 pr-11' : 'px-3.5',
            'placeholder:text-transparent',
            error
              ? 'border-red-500 focus:border-red-500 focus:shadow-[0_0_0_1px_rgb(239_68_68)]'
              : 'border-subtle focus:border-brand focus:shadow-[0_0_0_1px_rgb(var(--brand-primary))]',
            disabled ? 'cursor-not-allowed opacity-60' : '',
          ]
            .filter(Boolean)
            .join(' ')}
        />
        <label
          htmlFor={inputId}
          className={[
            'pointer-events-none absolute left-3 z-10 origin-left px-1',
            'bg-surface text-muted transition-all duration-150',
            'top-1/2 -translate-y-1/2 text-sm',
            'peer-focus:top-0 peer-focus:text-[11px] peer-focus:font-medium peer-focus:text-brand',
            'peer-[:not(:placeholder-shown)]:top-0 peer-[:not(:placeholder-shown)]:text-[11px] peer-[:not(:placeholder-shown)]:font-medium',
            error
              ? 'peer-focus:text-red-500 peer-[:not(:placeholder-shown)]:text-red-500'
              : 'peer-[:not(:placeholder-shown)]:text-secondary peer-focus:text-brand',
          ]
            .filter(Boolean)
            .join(' ')}
        >
          {label}
        </label>
        {hasEnd ? (
          <div className="pointer-events-auto absolute inset-y-0 right-0 z-10 flex w-11 items-center justify-center">
            {endAdornment}
          </div>
        ) : null}
      </div>
      {error ? (
        <p id={errorId} className="mt-1.5 text-xs text-red-500" role="alert">
          {error}
        </p>
      ) : null}
    </div>
  )
}
