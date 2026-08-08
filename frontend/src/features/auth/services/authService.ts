import { apiClient } from '../../../services/apiClient';
import type { AuthResponse, LoginRequest } from '../types/auth';

export const authService = {
  async login(request: LoginRequest): Promise<AuthResponse> {
    const { data } = await apiClient.post<AuthResponse>('/auth/login', request);
    return data;
  },
};
