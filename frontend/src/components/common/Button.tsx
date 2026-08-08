import type { ButtonHTMLAttributes, ReactNode } from 'react';
import { LoadingSpinner } from './LoadingSpinner';

interface ButtonProps extends ButtonHTMLAttributes<HTMLButtonElement> {
  children: ReactNode;
  isLoading?: boolean;
  leftIcon?: ReactNode;
}

export function Button({ children, className = '', disabled, isLoading = false, leftIcon, ...props }: ButtonProps) {
  return (
    <button
      className={`inline-flex h-12 w-full items-center justify-center gap-2 rounded-form bg-mis-primary px-4 text-sm font-semibold text-white shadow-sm transition duration-150 hover:bg-mis-deep disabled:cursor-not-allowed disabled:bg-slate-300 disabled:text-slate-600 ${className}`}
      disabled={disabled || isLoading}
      {...props}
    >
      {isLoading ? <LoadingSpinner className="h-4 w-4" /> : leftIcon}
      <span>{children}</span>
    </button>
  );
}
