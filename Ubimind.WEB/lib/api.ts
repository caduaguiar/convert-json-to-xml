import axios, { AxiosError } from 'axios';
import { API_CONFIG, UI_MESSAGES } from './constants';
import { generateXmlFileName } from './utils/file.utils';
import type { ConversionResponse, ApiError } from './types';

class ApiService {
  private readonly baseUrl: string;
  private readonly timeout: number;

  constructor() {
    this.baseUrl = API_CONFIG.BASE_URL;
    this.timeout = API_CONFIG.TIMEOUT;
  }

  private handleError(error: unknown): never {
    if (axios.isAxiosError(error)) {
      const axiosError = error as AxiosError<{ error?: string }>;
      const errorMessage =
        axiosError.response?.data?.error ||
        axiosError.message ||
        UI_MESSAGES.ERROR.CONVERSION_FAILED;

      throw new Error(errorMessage);
    }

    if (error instanceof SyntaxError) {
      throw new Error(UI_MESSAGES.ERROR.INVALID_JSON);
    }

    if (error instanceof Error) {
      throw error;
    }

    throw new Error(UI_MESSAGES.ERROR.UNEXPECTED_ERROR);
  }

  async convertJsonToXml(file: File): Promise<ConversionResponse> {
    try {
      const fileContent = await file.text();
      const jsonData = JSON.parse(fileContent);

      const response = await axios.post<string>(
        `${this.baseUrl}${API_CONFIG.ENDPOINTS.CONVERT}`,
        jsonData,
        {
          headers: {
            'Content-Type': 'application/json',
          },
          timeout: this.timeout,
        }
      );

      return {
        xml: response.data,
        fileName: generateXmlFileName(file.name),
      };
    } catch (error) {
      return this.handleError(error);
    }
  }
}

export const apiService = new ApiService();
export const convertJsonToXml = (file: File) => apiService.convertJsonToXml(file);
