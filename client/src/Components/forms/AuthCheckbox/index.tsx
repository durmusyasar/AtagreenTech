import { forwardRef, useId } from 'react'
import type { AuthCheckboxProps } from '@/Components/forms/AuthCheckbox/type'

/** Auth formları için marka uyumlu özel checkbox. */
export const AuthCheckbox = forwardRef<HTMLInputElement, AuthCheckboxProps>(
  function AuthCheckbox(
    { id, checked, onChange, onBlur, label, disabled = false, className = '' },
    ref,
  ) {
    const generatedId = useId()
    const inputId = id ?? generatedId

    // htmlFor yok: sarmalayan label + iç input aynı kontrolü iki kez tetiklemesin (Beni hatırla).
    return (
      <label
        className={`auth-checkbox ${disabled ? 'auth-checkbox--disabled' : ''} ${className}`.trim()}
      >
        <span className="relative inline-flex h-[18px] w-[18px] shrink-0 items-center justify-center">
          <input
            ref={ref}
            id={inputId}
            type="checkbox"
            checked={checked}
            disabled={disabled}
            onChange={(event) => onChange(event.target.checked)}
            onBlur={onBlur}
            className="auth-checkbox-input"
          />
          <span aria-hidden className="auth-checkbox-box">
            <svg
              className="auth-checkbox-mark"
              viewBox="0 0 16 16"
              fill="none"
              stroke="currentColor"
              strokeWidth="2.2"
              strokeLinecap="round"
              strokeLinejoin="round"
            >
              <path d="M3.5 8.2 6.6 11.2 12.5 4.8" />
            </svg>
          </span>
        </span>
        <span className="auth-checkbox-label">{label}</span>
      </label>
    )
  },
)
