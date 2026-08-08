import type { InputHTMLAttributes } from 'react';

interface CheckboxProps extends Omit<InputHTMLAttributes<HTMLInputElement>, 'type'> {
  label: string;
}

export function Checkbox({ id, label, className = '', ...props }: CheckboxProps) {
  const inputId = id ?? props.name;

  return (
    <label className="inline-flex cursor-pointer items-center gap-2 text-sm font-medium text-slate-600" htmlFor={inputId}>
      <input
        id={inputId}
        type="checkbox"
        className={`h-4 w-4 rounded border-mis-border text-mis-primary focus:ring-mis-blue ${className}`}
        {...props}
      />
      <span>{label}</span>
    </label>
  );
}
