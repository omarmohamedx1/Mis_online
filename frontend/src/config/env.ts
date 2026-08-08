const DEFAULT_API_URL = 'http://localhost:5000/api';

export const env = {
  apiUrl: import.meta.env.VITE_API_URL?.trim() || DEFAULT_API_URL,
} as const;
