import type { InputHTMLAttributes } from 'react';

interface TextInputProps extends InputHTMLAttributes<HTMLInputElement> {
  label: string;
  error?: string;
  hint?: string;
  containerClassName?: string;
}

export function TextInput({ id, label, error, hint, className = '', containerClassName = '', ...props }: TextInputProps) {
  const inputId = id ?? props.name;
  const errorId = error && inputId ? `${inputId}-error` : undefined;
  const isTechnicalValue = ['date', 'datetime-local', 'email', 'number', 'tel', 'time', 'url'].includes(props.type ?? '')
    || /(?:account|code|employeeNumber|hash|iban|id|number)$/i.test(props.name ?? '');

  return (
    <div className={`space-y-2 ${containerClassName}`}>
      <label className="block text-sm font-semibold text-slate-700" htmlFor={inputId}>
        {label}
      </label>
      <input
        id={inputId}
        aria-invalid={Boolean(error)}
        aria-describedby={errorId}
        dir={props.dir ?? (isTechnicalValue ? 'ltr' : undefined)}
        className={`h-12 w-full rounded-form border bg-white px-4 text-sm text-mis-ink transition placeholder:text-slate-400 focus:border-mis-blue focus:shadow-input focus:outline-none ${
          error ? 'border-red-400' : 'border-mis-border'
        } ${className}`}
        {...props}
      />
      {error ? (
        <p className="text-sm text-red-600" id={errorId}>
          {error}
        </p>
      ) : hint ? <p className="text-sm text-slate-500">{hint}</p> : null}
    </div>
  );
}
