import { useId } from 'react'
import type { ChangeEvent, MouseEvent } from 'react'
import type { FloatingInputProps } from '@/Components/FloatingInput/type'

/**
 * Material outlined floating label.
 * Elevated yüzey + net kenarlık; boşken etiket ortada, odak/doluyken üst border’da.
 * `autoClear` ile doluyken × temizleme düğmesi.
 */
export function FloatingInput({
  id,
  label,
  error,
  endAdornment,
  autoClear = false,
  clearLabel = 'Clear',
  className = '',
  disabled,
  value,
  onChange,
  ...inputProps
}: FloatingInputProps) {
  const generatedId = useId()
  const inputId = id ?? generatedId
  const errorId = error ? `${inputId}-error` : undefined

  const stringValue = value == null ? '' : String(value)
  const showClear = autoClear && !disabled && stringValue.length > 0
  const hasEndAdornment = Boolean(endAdornment)
  const rightSlotCount = (showClear ? 1 : 0) + (hasEndAdornment ? 1 : 0)
  const paddingClass =
    rightSlotCount >= 2 ? 'pl-3.5 pr-[4.75rem]' : rightSlotCount === 1 ? 'pl-3.5 pr-11' : 'px-3.5'

  const handleClear = (event: MouseEvent<HTMLButtonElement>) => {
    event.preventDefault()
    event.stopPropagation()
    if (disabled || !onChange) return

    const synthetic = {
      target: { value: '' },
      currentTarget: { value: '' },
    } as ChangeEvent<HTMLInputElement>
    onChange(synthetic)
  }

  return (
    <div className={`w-full min-w-0 max-w-full ${className}`.trim()}>
      <div className="relative w-full min-w-0">
        <input
          {...inputProps}
          id={inputId}
          value={value}
          disabled={disabled}
          onChange={onChange}
          placeholder=" "
          aria-invalid={Boolean(error)}
          aria-describedby={errorId}
          className={[
            'peer box-border appearance-none w-full max-w-full min-w-0 rounded-xl border',
            'h-12 bg-elevated text-sm text-primary outline-none',
            'shadow-[inset_0_1px_0_rgb(255_255_255/0.04)]',
            'transition-[border-color,box-shadow,background-color] duration-150',
            paddingClass,
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
            'bg-transparent text-secondary transition-all duration-150',
            'top-1/2 -translate-y-1/2 text-sm',
            'peer-focus:top-0 peer-focus:bg-surface peer-focus:text-[11px] peer-focus:font-medium peer-focus:text-brand',
            'peer-[:not(:placeholder-shown)]:top-0 peer-[:not(:placeholder-shown)]:bg-surface peer-[:not(:placeholder-shown)]:text-[11px] peer-[:not(:placeholder-shown)]:font-medium',
            error
              ? 'peer-focus:text-red-500 peer-[:not(:placeholder-shown)]:text-red-500'
              : 'peer-[:not(:placeholder-shown)]:text-secondary peer-focus:text-brand',
          ]
            .filter(Boolean)
            .join(' ')}
        >
          {label}
        </label>

        {rightSlotCount > 0 ? (
          <div
            className={[
              'pointer-events-auto absolute inset-y-0 right-0 z-10 flex items-center',
              rightSlotCount >= 2 ? 'gap-0.5 pr-1.5' : 'w-11 justify-center',
            ].join(' ')}
          >
            {showClear ? (
              <button
                type="button"
                onClick={handleClear}
                tabIndex={-1}
                aria-label={clearLabel}
                className={[
                  'box-border inline-flex h-8 w-8 shrink-0 appearance-none items-center justify-center',
                  'rounded-lg border-0 bg-transparent p-0 text-muted shadow-none',
                  'transition-colors hover:text-primary focus-visible:outline focus-visible:outline-2',
                  'focus-visible:outline-offset-2 focus-visible:outline-brand/40',
                ].join(' ')}
              >
                <svg className="h-3.5 w-3.5" viewBox="0 0 16 16" fill="none" aria-hidden>
                  <path
                    d="M4.2 4.2 11.8 11.8M11.8 4.2 4.2 11.8"
                    stroke="currentColor"
                    strokeWidth="1.8"
                    strokeLinecap="round"
                  />
                </svg>
              </button>
            ) : null}
            {hasEndAdornment ? endAdornment : null}
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
