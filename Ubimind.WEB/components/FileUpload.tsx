'use client';

import { FormEvent } from 'react';
import { useFileUpload } from '@/hooks/useFileUpload';
import { Alert } from './ui/Alert';
import { Button } from './ui/Button';
import { FileInput } from './ui/FileInput';
import { UI_MESSAGES } from '@/lib/constants';

export default function FileUpload() {
  const { file, state, error, handleFileChange, handleConvert, inputRef } = useFileUpload();

  const handleSubmit = async (e: FormEvent<HTMLFormElement>) => {
    e.preventDefault();
    await handleConvert();
  };

  const isLoading = state === 'loading';
  const isSuccess = state === 'success';
  const hasError = state === 'error' && error;

  return (
    <div className="w-full max-w-md mx-auto">
      <form onSubmit={handleSubmit} className="space-y-6">
        <FileInput
          ref={inputRef}
          id="file-input"
          label="Select JSON File"
          accept=".json"
          selectedFileName={file?.name}
          onChange={handleFileChange}
        />

        <Button
          type="submit"
          variant="primary"
          disabled={!file || isLoading}
          isLoading={isLoading}
        >
          {isLoading ? UI_MESSAGES.LOADING.CONVERTING : 'Convert to XML'}
        </Button>

        {hasError && (
          <Alert variant="error">{error}</Alert>
        )}

        {isSuccess && (
          <Alert variant="success">
            {UI_MESSAGES.SUCCESS.CONVERSION_COMPLETE}
          </Alert>
        )}
      </form>
    </div>
  );
}
