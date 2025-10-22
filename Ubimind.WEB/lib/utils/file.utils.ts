import { FILE_CONFIG, UI_MESSAGES } from '../constants';
import type { FileValidationResult } from '../types';

export const validateFile = (file: File | null): FileValidationResult => {
  if (!file) {
    return {
      isValid: false,
      error: UI_MESSAGES.ERROR.NO_FILE_SELECTED,
    };
  }

  const fileExtension = `.${file.name.split('.').pop()?.toLowerCase()}`;
  if (!FILE_CONFIG.ACCEPTED_EXTENSIONS.includes(fileExtension)) {
    return {
      isValid: false,
      error: UI_MESSAGES.ERROR.INVALID_EXTENSION,
    };
  }

  if (file.size > FILE_CONFIG.MAX_FILE_SIZE) {
    return {
      isValid: false,
      error: UI_MESSAGES.ERROR.FILE_TOO_LARGE,
    };
  }

  return { isValid: true };
};

export const downloadFile = (content: string, fileName: string, mimeType: string): void => {
  const blob = new Blob([content], { type: mimeType });
  const url = window.URL.createObjectURL(blob);
  const anchor = document.createElement('a');

  anchor.href = url;
  anchor.download = fileName;
  anchor.style.display = 'none';

  document.body.appendChild(anchor);
  anchor.click();
  document.body.removeChild(anchor);

  window.URL.revokeObjectURL(url);
};

export const generateXmlFileName = (jsonFileName: string): string => {
  return jsonFileName.replace(/\.json$/i, '.xml');
};
