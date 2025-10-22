import { ReactNode } from 'react';

interface AlertProps {
  variant: 'error' | 'success' | 'info';
  children: ReactNode;
  className?: string;
}

const variantStyles = {
  error: 'bg-red-50 dark:bg-red-900/20 border-red-200 dark:border-red-800 text-red-600 dark:text-red-400',
  success: 'bg-green-50 dark:bg-green-900/20 border-green-200 dark:border-green-800 text-green-600 dark:text-green-400',
  info: 'bg-blue-50 dark:bg-blue-900/20 border-blue-200 dark:border-blue-800 text-blue-600 dark:text-blue-400',
};

export const Alert = ({ variant, children, className = '' }: AlertProps) => {
  return (
    <div
      role="alert"
      className={`p-4 border rounded-lg ${variantStyles[variant]} ${className}`}
    >
      <p className="text-sm">{children}</p>
    </div>
  );
};
