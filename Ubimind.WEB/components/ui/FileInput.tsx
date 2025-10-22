import { ChangeEvent, InputHTMLAttributes, forwardRef } from 'react';

interface FileInputProps extends Omit<InputHTMLAttributes<HTMLInputElement>, 'type' | 'onChange'> {
  label: string;
  accept?: string;
  selectedFileName?: string;
  onChange: (file: File | null) => void;
}

export const FileInput = forwardRef<HTMLInputElement, FileInputProps>(
  ({ label, accept, selectedFileName, onChange, id, className = '', ...props }, ref) => {
    const handleChange = (e: ChangeEvent<HTMLInputElement>) => {
      const file = e.target.files?.[0] || null;
      onChange(file);
    };

    return (
      <div className={className}>
        <label
          htmlFor={id}
          className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2"
        >
          {label}
        </label>
        <input
          {...props}
          ref={ref}
          id={id}
          type="file"
          accept={accept}
          onChange={handleChange}
          className="block w-full text-sm text-gray-900 dark:text-gray-100 border border-gray-300 dark:border-gray-600 rounded-lg cursor-pointer bg-gray-50 dark:bg-gray-800 focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-transparent p-2.5"
        />
        {selectedFileName && (
          <p className="mt-2 text-sm text-gray-600 dark:text-gray-400">
            Selected: {selectedFileName}
          </p>
        )}
      </div>
    );
  }
);

FileInput.displayName = 'FileInput';
