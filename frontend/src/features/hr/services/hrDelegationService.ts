import { apiClient, requestApiFile, type ApiFile } from '../../../services/apiClient';
import type { DelegationDetails, DelegationQuery, PagedDelegations, SaveDelegationRequest } from '../types/delegation';

export const hrDelegationService = {
  async getPaged(query: DelegationQuery): Promise<PagedDelegations> { const { data } = await apiClient.get<PagedDelegations>('/hr/delegations', { params: { page: query.page, pageSize: query.pageSize, search: query.search || undefined, employeeId: query.employeeId || undefined, departmentId: query.departmentId || undefined, delegationTypeId: query.delegationTypeId || undefined, status: query.status || undefined, dateFrom: query.dateFrom || undefined, dateTo: query.dateTo || undefined, sortBy: query.sortBy, sortDirection: query.sortDirection } }); return data; },
  async getDetails(id: string): Promise<DelegationDetails> { const { data } = await apiClient.get<DelegationDetails>(`/hr/delegations/${id}`); return data; },
  async create(request: SaveDelegationRequest): Promise<DelegationDetails> { const { data } = await apiClient.post<DelegationDetails>('/hr/delegations', request); return data; },
  async update(id: string, request: Omit<SaveDelegationRequest, 'delegationNumber'>): Promise<DelegationDetails> { const { data } = await apiClient.put<DelegationDetails>(`/hr/delegations/${id}`, request); return data; },
  async cancel(id: string, reason: string): Promise<DelegationDetails> { const { data } = await apiClient.post<DelegationDetails>(`/hr/delegations/${id}/cancel`, { reason }); return data; },
  print(id: string): Promise<ApiFile> { return requestApiFile(`/hr/delegations/${id}/print`); },
};
