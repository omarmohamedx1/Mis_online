import type { InputHTMLAttributes } from 'react';

interface TextInputProps extends InputHTMLAttributes<HTMLInputElement> {
  label: string;
  error?: string;
}

export function TextInput({ id, label, error, className = '', ...props }: TextInputProps) {
  const inputId = id ?? props.name;
  const errorId = error && inputId ? `${inputId}-error` : undefined;

  return (
    <div className="space-y-2">
      <label className="block text-sm font-semibold text-slate-700" htmlFor={inputId}>
        {label}
      </label>
      <input
        id={inputId}
        aria-invalid={Boolean(error)}
        aria-describedby={errorId}
        className={`h-12 w-full rounded-form border bg-white px-4 text-sm text-mis-ink transition placeholder:text-slate-400 focus:border-mis-blue focus:shadow-input focus:outline-none ${
          error ? 'border-red-400' : 'border-mis-border'
        } ${className}`}
        {...props}
      />
      {error ? (
        <p className="text-sm text-red-600" id={errorId}>
          {error}
        </p>
      ) : null}
    </div>
  );
}
