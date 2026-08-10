import { apiClient } from '../../../services/apiClient';
import type { HrDashboardSummary } from '../types/dashboard';

export const hrDashboardService = {
  async getSummary(): Promise<HrDashboardSummary> {
    const { data } = await apiClient.get<HrDashboardSummary>('/hr/dashboard');
    return data;
  },
};
