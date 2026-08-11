import { forwardRef, useId, type InputHTMLAttributes, type ReactNode } from 'react';
import { FormField, formControlClass, joinDescribedBy } from './FormField';

export interface DateInputProps extends Omit<InputHTMLAttributes<HTMLInputElement>, 'type'> {
  containerClassName?: string;
  error?: ReactNode;
  hint?: ReactNode;
  label: ReactNode;
}

export const DateInput = forwardRef<HTMLInputElement, DateInputProps>(function DateInput(
  {
    'aria-describedby': ariaDescribedBy,
    className = '',
    containerClassName = '',
    error,
    hint,
    id,
    label,
    name,
    required,
    ...props
  },
  ref,
) {
  const generatedId = useId();
  const inputId = id ?? name ?? generatedId;
  const errorId = error ? `${inputId}-error` : undefined;
  const hintId = hint ? `${inputId}-hint` : undefined;

  return (
    <FormField className={containerClassName} error={error} errorId={errorId} hint={hint} hintId={hintId} inputId={inputId} label={label} required={required}>
      <input
        aria-describedby={joinDescribedBy(ariaDescribedBy, hintId, errorId)}
        aria-invalid={error ? true : undefined}
        className={`h-12 px-4 ${formControlClass} ${error ? 'border-red-400' : 'border-mis-border'} ${className}`}
        id={inputId}
        name={name}
        ref={ref}
        required={required}
        type="date"
        {...props}
      />
    </FormField>
  );
});
