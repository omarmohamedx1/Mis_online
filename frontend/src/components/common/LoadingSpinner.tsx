interface LoadingSpinnerProps {
  className?: string;
}

export function LoadingSpinner({ className = 'h-5 w-5' }: LoadingSpinnerProps) {
  return (
    <span
      aria-hidden="true"
      className={`${className} inline-block animate-spin rounded-full border-2 border-current border-r-transparent`}
    />
  );
}
