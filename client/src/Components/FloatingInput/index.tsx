import { useId } from 'react'
import type { FloatingInputProps } from '@/Components/FloatingInput/type'

/**
 * Material outlined floating label.
 * Elevated yüzey + net kenarlık; boşken etiket ortada, odak/doluyken üst border’da.
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
            'peer box-border appearance-none w-full max-w-full min-w-0 rounded-xl border',
            'h-12 bg-elevated text-sm text-primary outline-none',
            'shadow-[inset_0_1px_0_rgb(255_255_255/0.04)]',
            'transition-[border-color,box-shadow,background-color] duration-150',
            hasEnd ? 'pl-3.5 pr-11' : 'px-3.5',
            'placeholder:text-transparent',
            'hover:border-[rgb(var(--border-subtle)/0.95)] dark:hover:border-white/20',
            error
              ? 'border-red-500 focus:border-red-500 focus:shadow-[0_0_0_3px_rgb(239_68_68/0.2)]'
              : 'border-subtle focus:border-brand focus:shadow-[0_0_0_3px_rgb(var(--brand-primary)/0.22)]',
            disabled ? 'cursor-not-allowed opacity-60' : '',
          ]
            .filter(Boolean)
            .join(' ')}
        />
        <label
          htmlFor={inputId}
          className={[
            'pointer-events-none absolute left-3 z-10 origin-left px-1.5',
            // Kesik outline: kart rengi (surface) ile border’ı keser
            'bg-surface text-secondary transition-all duration-150',
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
