import { forwardRef, useId, useRef } from 'react'
import type { ChangeEvent, MouseEvent, Ref } from 'react'
import type { FloatingInputProps } from '@/Components/FloatingInput/type'

function assignRef<T>(ref: Ref<T> | undefined, value: T | null) {
  if (!ref) return
  if (typeof ref === 'function') {
    ref(value)
    return
  }
  ref.current = value
}

/**
 * Material outlined floating label.
 * Elevated yüzey + net kenarlık; boşken etiket ortada, odak/doluyken üst border’da.
 * `autoClear` ile doluyken × temizleme düğmesi.
 */
export const FloatingInput = forwardRef<HTMLInputElement, FloatingInputProps>(
  function FloatingInput(
    {
      id,
      label,
      error,
      endAdornment,
      autoClear = false,
      clearLabel = 'Clear',
      className = '',
      disabled,
      readOnly,
      value,
      onChange,
      ...inputProps
    },
    ref,
  ) {
    const generatedId = useId()
    const inputId = id ?? generatedId
    const errorId = error ? `${inputId}-error` : undefined
    const inputRef = useRef<HTMLInputElement | null>(null)

    const stringValue = value == null ? '' : String(value)
    const showClear = autoClear && !disabled && !readOnly && stringValue.length > 0
    const hasEndAdornment = Boolean(endAdornment)
    const rightSlotCount = (showClear ? 1 : 0) + (hasEndAdornment ? 1 : 0)
    const paddingClass =
      rightSlotCount >= 2 ? 'pl-3.5 pr-[4.75rem]' : rightSlotCount === 1 ? 'pl-3.5 pr-11' : 'px-3.5'

    const handleClear = (event: MouseEvent<HTMLButtonElement>) => {
      event.preventDefault()
      event.stopPropagation()
      if (disabled || readOnly || !onChange) return

      const synthetic = {
        target: { value: '' },
        currentTarget: { value: '' },
      } as ChangeEvent<HTMLInputElement>
      onChange(synthetic)
      inputRef.current?.focus()
    }

    return (
      <div className={`w-full min-w-0 max-w-full ${className}`.trim()}>
        <div className="relative w-full min-w-0">
          <input
            {...inputProps}
            ref={(node) => {
              inputRef.current = node
              assignRef(ref, node)
            }}
            id={inputId}
            value={stringValue}
            disabled={disabled}
            readOnly={readOnly}
            onChange={onChange}
            placeholder=" "
            aria-invalid={Boolean(error)}
            aria-describedby={errorId}
            className={`floating-input ${error ? 'floating-input--error' : 'floating-input--ok'} ${paddingClass}`}
          />
          <label htmlFor={inputId} className="floating-input-label">
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
                  className="floating-input-clear"
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
          <p id={errorId} className="floating-input-error" role="alert">
            {error}
          </p>
        ) : null}
      </div>
    )
  },
)
