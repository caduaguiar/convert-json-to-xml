import { useState, useCallback, useRef } from 'react';
import { convertJsonToXml } from '@/lib/api';
import { validateFile, downloadFile } from '@/lib/utils/file.utils';
import { FILE_CONFIG, UI_MESSAGES } from '@/lib/constants';
import type { ConversionState } from '@/lib/types';

interface UseFileUploadReturn {
  file: File | null;
  state: ConversionState;
  error: string | null;
  handleFileChange: (file: File | null) => void;
  handleConvert: () => Promise<void>;
  clearFile: () => void;
  inputRef: React.RefObject<HTMLInputElement>;
}

export const useFileUpload = (): UseFileUploadReturn => {
  const [file, setFile] = useState<File | null>(null);
  const [state, setState] = useState<ConversionState>('idle');
  const [error, setError] = useState<string | null>(null);
  const inputRef = useRef<HTMLInputElement>(null);

  const clearFile = useCallback(() => {
    setFile(null);
    setError(null);
    setState('idle');
    if (inputRef.current) {
      inputRef.current.value = '';
    }
  }, []);

  const handleFileChange = useCallback((selectedFile: File | null) => {
    if (!selectedFile) {
      clearFile();
      return;
    }

    const validation = validateFile(selectedFile);

    if (!validation.isValid) {
      setError(validation.error || UI_MESSAGES.ERROR.INVALID_EXTENSION);
      setFile(null);
      setState('error');
      return;
    }

    setFile(selectedFile);
    setError(null);
    setState('idle');
  }, [clearFile]);

  const handleConvert = useCallback(async () => {
    const validation = validateFile(file);

    if (!validation.isValid) {
      setError(validation.error || UI_MESSAGES.ERROR.NO_FILE_SELECTED);
      setState('error');
      return;
    }

    setState('loading');
    setError(null);

    try {
      const result = await convertJsonToXml(file!);

      downloadFile(result.xml, result.fileName, FILE_CONFIG.MIME_TYPES.XML);

      setState('success');
      setError(null);

      setTimeout(() => {
        clearFile();
      }, 3000);
    } catch (err) {
      const errorMessage = err instanceof Error ? err.message : UI_MESSAGES.ERROR.UNEXPECTED_ERROR;
      setError(errorMessage);
      setState('error');
    }
  }, [file, clearFile]);

  return {
    file,
    state,
    error,
    handleFileChange,
    handleConvert,
    clearFile,
    inputRef,
  };
};
