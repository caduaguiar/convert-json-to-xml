export const API_CONFIG = {
  BASE_URL: process.env.NEXT_PUBLIC_API_URL || 'http://localhost:5000',
  ENDPOINTS: {
    CONVERT: '/api/conversion/json-to-xml',
  },
  TIMEOUT: 30000,
} as const;

export const FILE_CONFIG = {
  ACCEPTED_EXTENSIONS: ['.json'],
  MAX_FILE_SIZE: 10 * 1024 * 1024, // 10MB
  MIME_TYPES: {
    JSON: 'application/json',
    XML: 'application/xml',
  },
} as const;

export const UI_MESSAGES = {
  ERROR: {
    INVALID_EXTENSION: 'Please select a JSON file',
    NO_FILE_SELECTED: 'Please select a file',
    INVALID_JSON: 'Invalid JSON file format',
    CONVERSION_FAILED: 'Failed to convert file',
    FILE_TOO_LARGE: 'File size exceeds 10MB limit',
    UNEXPECTED_ERROR: 'An unexpected error occurred',
  },
  SUCCESS: {
    CONVERSION_COMPLETE: 'File converted successfully and download started!',
  },
  LOADING: {
    CONVERTING: 'Converting...',
  },
} as const;
