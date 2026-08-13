import { apiClient } from '../../../services/apiClient';
import type { UserProfile } from '../types/auth';

export const profileService = {
  async get(): Promise<UserProfile> { return (await apiClient.get<UserProfile>('/profile')).data; },
  async changeEmail(value: { newEmail: string; currentPassword: string }): Promise<UserProfile> { return (await apiClient.put<UserProfile>('/profile/email', value)).data; },
  async changePassword(value: { currentPassword: string; newPassword: string; confirmPassword: string }): Promise<void> { await apiClient.put('/profile/password', value); },
};
