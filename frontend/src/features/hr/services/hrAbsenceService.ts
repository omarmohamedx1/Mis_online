import { apiClient } from '../../../services/apiClient';
import type { AbsenceDetails, PagedAbsences, ReviewAbsencePayrollImpactRequest, SaveAbsenceRequest } from '../types/absence';

export interface AbsenceQuery { page: number; pageSize: number; search: string; departmentId: string; date: string; status: string; }
export const hrAbsenceService = {
  async getAbsences(query: AbsenceQuery): Promise<PagedAbsences> { const { data } = await apiClient.get<PagedAbsences>('/hr/absences', { params: { page: query.page, pageSize: query.pageSize, search: query.search || undefined, departmentId: query.departmentId || undefined, date: query.date || undefined, status: query.status } }); return data; },
  async getAbsence(id: string): Promise<AbsenceDetails> { const { data } = await apiClient.get<AbsenceDetails>(`/hr/absences/${id}`); return data; },
  async createAbsence(request: SaveAbsenceRequest): Promise<AbsenceDetails> { const { data } = await apiClient.post<AbsenceDetails>('/hr/absences', request); return data; },
  async updateAbsence(id: string, request: SaveAbsenceRequest): Promise<AbsenceDetails> { const { data } = await apiClient.put<AbsenceDetails>(`/hr/absences/${id}`, request); return data; },
  async deleteAbsence(id: string): Promise<void> { await apiClient.delete(`/hr/absences/${id}`); },
  async reviewPayrollImpact(id: string, request: ReviewAbsencePayrollImpactRequest): Promise<AbsenceDetails> { const { data } = await apiClient.patch<AbsenceDetails>(`/hr/absences/${id}/payroll-impact`, request); return data; },
};
