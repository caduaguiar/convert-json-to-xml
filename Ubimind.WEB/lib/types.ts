export interface ConversionResponse {
  xml: string;
  fileName: string;
}

export interface ApiError {
  message: string;
  code?: string;
  details?: unknown;
}

export interface FileValidationResult {
  isValid: boolean;
  error?: string;
}

export type ConversionState = 'idle' | 'loading' | 'success' | 'error';
