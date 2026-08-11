import { Eye, EyeOff } from 'lucide-react';
import { useState, type InputHTMLAttributes } from 'react';
import { useLocalization } from '../../context/LocalizationContext';

interface PasswordInputProps extends InputHTMLAttributes<HTMLInputElement> {
  label: string;
  error?: string;
}

export function PasswordInput({ id, label, error, className = '', ...props }: PasswordInputProps) {
  const { t } = useLocalization();
  const [isVisible, setIsVisible] = useState(false);
  const inputId = id ?? props.name;
  const errorId = error && inputId ? `${inputId}-error` : undefined;
  const Icon = isVisible ? EyeOff : Eye;

  return (
    <div className="space-y-2">
      <label className="block text-sm font-semibold text-slate-700" htmlFor={inputId}>
        {label}
      </label>
      <div className="relative">
        <input
          id={inputId}
          type={isVisible ? 'text' : 'password'}
          aria-invalid={Boolean(error)}
          aria-describedby={errorId}
          className={`h-12 w-full rounded-form border bg-white py-3 pe-12 ps-4 text-sm text-mis-ink transition placeholder:text-slate-400 focus:border-mis-blue focus:shadow-input focus:outline-none ${
            error ? 'border-red-400' : 'border-mis-border'
          } ${className}`}
          {...props}
        />
        <button
          aria-label={isVisible ? t('hidePassword') : t('showPassword')}
          className="absolute end-2 top-1/2 inline-flex h-9 w-9 -translate-y-1/2 items-center justify-center rounded-lg text-slate-500 transition hover:bg-mis-pale hover:text-mis-primary"
          onClick={() => setIsVisible((current) => !current)}
          title={isVisible ? t('hidePassword') : t('showPassword')}
          type="button"
        >
          <Icon className="h-4 w-4" aria-hidden="true" />
        </button>
      </div>
      {error ? (
        <p className="text-sm text-red-600" id={errorId}>
          {error}
        </p>
      ) : null}
    </div>
  );
}
