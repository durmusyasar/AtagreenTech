import { useId } from 'react'
import type { AuthCheckboxProps } from '@/Components/forms/AuthCheckbox/type'

/** Auth formları için marka uyumlu özel checkbox. */
export function AuthCheckbox({ id, checked, onChange, label, className = '' }: AuthCheckboxProps) {
  const generatedId = useId()
  const inputId = id ?? generatedId

  return (
    <label
      htmlFor={inputId}
      className={`group inline-flex cursor-pointer items-center gap-2.5 select-none ${className}`.trim()}
    >
      <span className="relative inline-flex h-[18px] w-[18px] shrink-0 items-center justify-center">
        <input
          id={inputId}
          type="checkbox"
          checked={checked}
          onChange={(event) => onChange(event.target.checked)}
          className="peer sr-only"
        />
        <span
          aria-hidden
          className={[
            'box-border flex h-[18px] w-[18px] items-center justify-center rounded-md border',
            'bg-elevated transition-all duration-150',
            'peer-focus-visible:outline peer-focus-visible:outline-2 peer-focus-visible:outline-offset-2 peer-focus-visible:outline-brand/40',
            'group-hover:border-brand/50',
            checked
              ? 'border-brand bg-brand text-white shadow-[0_0_0_3px_rgb(var(--brand-primary)/0.18)]'
              : 'border-subtle text-transparent',
          ].join(' ')}
        >
          <svg
            className={`h-3 w-3 transition-transform duration-150 ${checked ? 'scale-100' : 'scale-75'}`}
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
      <span className="text-xs font-medium text-secondary transition-colors group-hover:text-primary">
        {label}
      </span>
    </label>
  )
}
