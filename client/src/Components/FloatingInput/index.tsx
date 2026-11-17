import { useId } from 'react'
import type { FloatingInputProps } from '@/Components/FloatingInput/type'

/**
 * Kayan etiketli (floating label) metin alanı.
 * Auth ve form ekranlarında ortak girdi standardı.
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
    <div className={`w-full ${className}`.trim()}>
      <div className="relative">
        <input
          {...inputProps}
          id={inputId}
          value={value}
          disabled={disabled}
          placeholder=" "
          aria-invalid={Boolean(error)}
          aria-describedby={errorId}
          className={[
            'peer appearance-none w-full rounded-xl border bg-elevated text-sm text-primary',
            'pt-5 pb-2 outline-none transition-all duration-150',
            hasEnd ? 'pl-3.5 pr-11' : 'px-3.5',
            'placeholder:text-transparent',
            'focus:ring-2',
            error
              ? 'border-red-500 focus:border-red-500 focus:ring-red-500/25'
              : 'border-subtle focus:border-brand focus:ring-brand/30',
            disabled ? 'cursor-not-allowed opacity-60' : '',
          ]
            .filter(Boolean)
            .join(' ')}
        />
        <label
          htmlFor={inputId}
          className={[
            'pointer-events-none absolute left-3.5 origin-left text-muted transition-all duration-150',
            'top-1/2 -translate-y-1/2 text-sm',
            'peer-focus:top-2 peer-focus:translate-y-0 peer-focus:text-[11px] peer-focus:font-medium peer-focus:text-brand',
            'peer-[:not(:placeholder-shown)]:top-2 peer-[:not(:placeholder-shown)]:translate-y-0 peer-[:not(:placeholder-shown)]:text-[11px] peer-[:not(:placeholder-shown)]:font-medium',
            error ? 'peer-[:not(:placeholder-shown)]:text-red-500 peer-focus:text-red-500' : '',
          ]
            .filter(Boolean)
            .join(' ')}
        >
          {label}
        </label>
        {hasEnd ? (
          <div className="absolute right-2.5 top-1/2 z-10 -translate-y-1/2">{endAdornment}</div>
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
